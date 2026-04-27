using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VectorBLFTools
{
    public class BinlogReadWrite
    {

        //throw Exception
        public static List<MessageBase> readBLF(string path) {

            if (!File.Exists(path)) { 
            
                throw new FileNotFoundException();
            }

            List<MessageBase> readMessageList = new List<MessageBase>();
            IntPtr fileHandle = BLFAPI.BLCreateFileW(path, GENERIC.GENERIC_READ);
            if (fileHandle == IntPtr.Zero) { 
                throw new Exception(String.Format("Read BLF file error.File Path:{0}",path));
            }
            VBLFileStatistics myFileStatistics;
            myFileStatistics.mStatisticsSize = 28;
            int retval = BLFAPI.BLGetFileStatistics(fileHandle, out myFileStatistics);

            VBLObjectHeaderBase headerBase;
            

            while (retval == 1)
            {
                retval = BLFAPI.BLPeekObject(fileHandle, out headerBase);
                if (retval == 1)
                {
                    if (headerBase.mObjectType == (uint)BLFObjectType.BL_OBJ_TYPE_CAN_MESSAGE)
                    {
                        int structSize = Marshal.SizeOf<VBLCANMessage>();
                        IntPtr pMsg = Marshal.AllocHGlobal(structSize);
                        VBLCANMessage msg = new VBLCANMessage();
                        msg.mHeader.mBase = headerBase;
                        Marshal.StructureToPtr(msg, pMsg, false);
                        retval = BLFAPI.BLReadObjectSecure(fileHandle, pMsg, (UIntPtr)structSize);
                        if (retval == 1) {
                            VBLCANMessage retMsg = Marshal.PtrToStructure<VBLCANMessage>(pMsg);
                            readMessageList.Add(new CANMessage(retMsg.mChannel, retMsg.mID,retMsg.mData, msg.mHeader.mObjectTimeStamp / 1000000000.0));
                        }
                        
                        Marshal.FreeHGlobal(pMsg);
                        
                    }
                    else if (headerBase.mObjectType == (uint)BLFObjectType.BL_OBJ_TYPE_CAN_FD_MESSAGE_64)
                    {
                        int structSize = Marshal.SizeOf<VBLCANFDMessage64>();
                        IntPtr pMsg = Marshal.AllocHGlobal(structSize);
                        VBLCANFDMessage64 msg = new VBLCANFDMessage64();
                        msg.mHeader.mBase = headerBase;
                        Marshal.StructureToPtr(msg, pMsg, false);
                        retval = BLFAPI.BLReadObjectSecure(fileHandle, pMsg, (UIntPtr)structSize);
                        if (retval == 1)
                        {
                            VBLCANFDMessage64 retMsg = Marshal.PtrToStructure<VBLCANFDMessage64>(pMsg);
                            readMessageList.Add(new CANFDMessage(retMsg.mChannel, retMsg.mID, 1 , retMsg.mData, msg.mHeader.mObjectTimeStamp / 1000000000.0));
                        }
                        Marshal.FreeHGlobal(pMsg);
                    }
                }
            }
            BLFAPI.BLCloseHandle(fileHandle);
            return readMessageList;
        }

        public static bool writeBLF(string path,List<MessageBase> messageList) {

            IntPtr fileHandle = BLFAPI.BLCreateFileW(path, GENERIC.GENERIC_WRITE);

            int retval = BLFAPI.BLSetApplication(fileHandle, BLAppID.BL_APPID_CANALYZER, 3, 0, 1);
            int timeSize = Marshal.SizeOf<SYSTEMTIME>();
            IntPtr timePtr = Marshal.AllocHGlobal(timeSize);
            SYSTEMTIME systemTime = new SYSTEMTIME();
            DateTime now = DateTime.Now;
            systemTime.wYear = (UInt16)now.Year;
            systemTime.wMonth = (UInt16)now.Month;
            systemTime.wDay = (UInt16)now.Day;
            systemTime.wHour = (UInt16)now.Hour;
            systemTime.wMinute = (UInt16)now.Minute;
            systemTime.wSecond = (UInt16)now.Second;
            Marshal.StructureToPtr(systemTime, timePtr, false);
            retval = BLFAPI.BLSetMeasurementStartTime(fileHandle, timePtr);
            Marshal.FreeHGlobal(timePtr);

            foreach (MessageBase msg in messageList) {
                CANType type = msg.canType;
                switch (type) {
                    case CANType.CAN:
                        CANMessage canMsg = (CANMessage)msg;
                        int msgSize = Marshal.SizeOf<VBLCANMessage>();
                        IntPtr msgPtr = Marshal.AllocHGlobal(msgSize);
                        VBLCANMessage canMessage = new VBLCANMessage();
                        canMessage.mHeader.mBase.mSignature = BLFAPI.BL_OBJ_SIGNATURE;
                        canMessage.mHeader.mBase.mHeaderSize = (UInt16)Marshal.SizeOf(typeof(VBLObjectHeader));
                        canMessage.mHeader.mBase.mHeaderVersion = 1;
                        canMessage.mHeader.mObjectTimeStamp = (ulong)(1000000000L * canMsg.timeStamp);
                        canMessage.mHeader.mBase.mObjectSize = (UInt16)Marshal.SizeOf(typeof(VBLCANMessage));
                        canMessage.mHeader.mBase.mObjectType = (uint)BLFObjectType.BL_OBJ_TYPE_CAN_MESSAGE;
                        canMessage.mHeader.mObjectFlags = (uint)ObjectFlag.BL_OBJ_FLAG_TIME_ONE_NANS;
                        canMessage.mChannel = (UInt16)(canMsg.channel + 1);
                        canMessage.mFlags = 0;
                        canMessage.mDLC = canMsg.DLC;
                        canMessage.mID = canMsg.ID;
                        canMessage.mData = canMsg.data;
                        Marshal.StructureToPtr(canMessage, msgPtr, false);
                        retval = BLFAPI.BLWriteObject(fileHandle, msgPtr);
                        Marshal.FreeHGlobal(msgPtr);
                        if (retval != 1)
                        {
                            throw new Exception("Save message fail! The details of the message that failed to save are as follows:" + canMsg.ToString());
                        }
                        
                        break;
                    case CANType.CANFD:
                        CANFDMessage canFDMsg = (CANFDMessage)msg;

                        int msgFDSize = Marshal.SizeOf<VBLCANFDMessage64>();
                        IntPtr msgFDPtr = Marshal.AllocHGlobal(msgFDSize);
                        VBLCANFDMessage64 canFDMessage = new VBLCANFDMessage64();
                        canFDMessage.mHeader.mBase.mSignature = BLFAPI.BL_OBJ_SIGNATURE;
                        canFDMessage.mHeader.mBase.mHeaderSize = (UInt16)Marshal.SizeOf(typeof(VBLCANFDMessage64));
                        canFDMessage.mHeader.mBase.mHeaderVersion = 1;
                        canFDMessage.mHeader.mObjectTimeStamp = (ulong)(1000000000L * canFDMsg.timeStamp);
                        canFDMessage.mHeader.mBase.mObjectSize = (UInt16)Marshal.SizeOf(typeof(VBLCANFDMessage64));
                        canFDMessage.mHeader.mBase.mObjectType = (uint)BLFObjectType.BL_OBJ_TYPE_CAN_FD_MESSAGE_64;
                        canFDMessage.mHeader.mObjectFlags = (uint)ObjectFlag.BL_OBJ_FLAG_TIME_ONE_NANS;
                        canFDMessage.mChannel = (byte)(canFDMsg.channel + 1);
                        canFDMessage.mFlags = 0x3000;
                        if (canFDMsg.messageFlag == MessageFlag.MSG_EXT)
                        {
                            canFDMessage.mID = 0x80000000 | canFDMsg.ID;
                        }
                        else {
                            canFDMessage.mID = canFDMsg.ID;
                        }
                        
                        canFDMessage.mValidDataBytes = (byte)canFDMsg.data.Length;
                        byte[] data = new byte[64];
                        
                        Array.Copy(canFDMsg.data, 0, data, 0, canFDMsg.data.Length);
                        canFDMessage.mData = data;
                        canFDMessage.mDLC = (byte)BinlogReadWrite.getDLC(canFDMsg.data.Length);
                        Marshal.StructureToPtr(canFDMessage, msgFDPtr, false);

                        retval = BLFAPI.BLWriteObject(fileHandle, msgFDPtr);
                        Marshal.FreeHGlobal(msgFDPtr);
                        if (retval != 1) {
                            throw new Exception("Save message fail! The details of the message that failed to save are as follows:"+ canFDMsg.ToString());
                        }
                        break;
                    default:
                        break;
                }
            }
            BLFAPI.BLCloseHandle(fileHandle);
            return true;
        }

        private static int getDLC(int dataLength) {
            if (dataLength >= 0 && dataLength <= 8)
            {
                return 8;
            }
            else if (dataLength > 8 && dataLength <= 12)
            {
                return 9;
            }
            else if (dataLength > 12 && dataLength <= 16)
            {
                return 10;

            }
            else if (dataLength > 16 && dataLength <= 20)
            {
                return 11;
            }
            else if (dataLength > 20 && dataLength <= 24)
            {
                return 12;
            }
            else if (dataLength > 24 && dataLength <= 32)
            {
                return 13;
            }
            else if (dataLength > 32 && dataLength <= 48)
            {
                return 14;
            }
            else if (dataLength > 48 && dataLength <= 64)
            {
                return 15;
            }
            else {
                return 15;
            }
            
        }

        private static int getValidDataBytes(int MsgLenght)
        {

            if (MsgLenght > 8 && MsgLenght <= 12)
            {
                return 12;
            }
            else if (MsgLenght > 12 && MsgLenght <= 16)
            {
                return 16;
            }
            else if (MsgLenght > 16 && MsgLenght <= 20)
            {
                return 20;
            }
            else if (MsgLenght > 20 && MsgLenght <= 24)
            {
                return 24;
            }
            else if (MsgLenght > 24 && MsgLenght <= 32)
            {
                return 32;
            }
            else if (MsgLenght > 32 && MsgLenght <= 48)
            {
                return 48;
            }
            else if (MsgLenght > 48 && MsgLenght <= 64)
            {
                return 64;
            }
            else
            {
                return 8;
            }
        }
    }
}
