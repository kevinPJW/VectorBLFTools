using Microsoft.VisualStudio.TestTools.UnitTesting;
using VectorBLFTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace VectorBLFTools.Tests
{
    [TestClass()]
    public class BinlogReaderTests
    {
        [TestMethod()]
        public void loadFileTest()
        {
            IntPtr fileHandle = BLFAPI.BLCreateFileW("D:\\tzhs\\leftLog.blf", GENERIC.GENERIC_READ);


            //读取文件信息
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
                        VBLCANMessage retMsg = Marshal.PtrToStructure<VBLCANMessage>(pMsg);
                        double timems = retMsg.mHeader.mObjectTimeStamp / 1000000000.0;
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
                        VBLCANFDMessage64 retMsg = Marshal.PtrToStructure<VBLCANFDMessage64>(pMsg);
                        Marshal.FreeHGlobal(pMsg);
                    }
                }
            }

            uint res = BLFAPI.BLCloseHandle(fileHandle);

        }



        [TestMethod()]
        public void writeCANBLFTests()
        {
            File.Delete("D:\\tzhs\\testWrite.blf");

            IntPtr fileHandle = BLFAPI.BLCreateFileW("D:\\tzhs\\testWrite.blf", GENERIC.GENERIC_WRITE);

            int retval = BLFAPI.BLSetApplication(fileHandle, BLAppID.BL_APPID_CANALYZER,3,0,1);
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







            int msgSize = Marshal.SizeOf<VBLCANMessage>();
            IntPtr msgPtr = Marshal.AllocHGlobal(msgSize);
            VBLCANMessage canMessage = new VBLCANMessage();
            canMessage.mHeader.mBase.mSignature = BLFAPI.BL_OBJ_SIGNATURE;
            canMessage.mHeader.mBase.mHeaderSize = (UInt16)Marshal.SizeOf(typeof(VBLObjectHeader));
            canMessage.mHeader.mBase.mHeaderVersion = 1;
            canMessage.mHeader.mObjectTimeStamp = (10 * 1000000000L);
            canMessage.mHeader.mBase.mObjectSize = (UInt16)Marshal.SizeOf(typeof(VBLCANMessage));
            canMessage.mHeader.mBase.mObjectType = (uint)BLFObjectType.BL_OBJ_TYPE_CAN_MESSAGE;
            canMessage.mHeader.mObjectFlags = (uint)ObjectFlag.BL_OBJ_FLAG_TIME_ONE_NANS;
            canMessage.mChannel = 1;
            canMessage.mFlags = 0;
            canMessage.mDLC = 8;
            canMessage.mID = 0x100;
            byte[]data = new byte[8] {0x00,0x11,0x22,0x33,0x44,0x55,0x66,0x77 };
            canMessage.mData = data;

            Marshal.StructureToPtr(canMessage, msgPtr, false);

            retval = BLFAPI.BLWriteObject(fileHandle, msgPtr);

            uint res = BLFAPI.BLCloseHandle(fileHandle);
        }

        [TestMethod()]
        public void writeCANFBBLFTests()
        {
            File.Delete("D:\\tzhs\\testWrite.blf");

            IntPtr fileHandle = BLFAPI.BLCreateFileW("D:\\tzhs\\testWrite.blf", GENERIC.GENERIC_WRITE);

            int retval = BLFAPI.BLSetApplication(fileHandle, BLAppID.BL_APPID_CANOE, 3, 0, 1);
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


            int msgSize = Marshal.SizeOf<VBLCANFDMessage64>();
            IntPtr msgPtr = Marshal.AllocHGlobal(msgSize);
            VBLCANFDMessage64 canFDMessage = new VBLCANFDMessage64();
            canFDMessage.mHeader.mBase.mSignature = BLFAPI.BL_OBJ_SIGNATURE;
            canFDMessage.mHeader.mBase.mHeaderSize = (UInt16)Marshal.SizeOf(typeof(VBLCANFDMessage64));
            canFDMessage.mHeader.mBase.mHeaderVersion = 1;
            canFDMessage.mHeader.mObjectTimeStamp = (10 * 1000000000L);
            canFDMessage.mHeader.mBase.mObjectSize = (UInt16)Marshal.SizeOf(typeof(VBLCANFDMessage64));
            canFDMessage.mHeader.mBase.mObjectType = (uint)BLFObjectType.BL_OBJ_TYPE_CAN_FD_MESSAGE_64;
            canFDMessage.mHeader.mObjectFlags = (uint)ObjectFlag.BL_OBJ_FLAG_TIME_ONE_NANS;
            canFDMessage.mChannel = 1;
            canFDMessage.mFlags = 0x3000;
            
            canFDMessage.mID = 0x101;
            canFDMessage.mValidDataBytes = 0x10;
            byte[] data = new byte[64];
            byte[] tmp = new byte[16] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 };
            Array.Copy(tmp, 0, data, 0, 16);
            canFDMessage.mData = data;
            //canFDMessage.mDLC = (byte)BinlogReadWrite.getDLC(tmp.Length);

            Marshal.StructureToPtr(canFDMessage, msgPtr, false);

            retval = BLFAPI.BLWriteObject(fileHandle, msgPtr);

            uint res = BLFAPI.BLCloseHandle(fileHandle);
        }


        [TestMethod()]
        public void ClassWriteTest() {
            string filePath = "D:\\tzhs\\testWrite.blf";
            File.Delete(filePath);

            //uint channle_,byte DLC_ ,uint ID_, byte[]data_
            CANMessage canMsg = new CANMessage(1,0x10,new byte[] {0x00,0x11,0x22,0x33,0x44,0x55,0x66,0x77 },20);

            
            
            CANFDMessage canFDMsg = new CANFDMessage(2,0x57F,0,new byte[16] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 , 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 },0.2);
            List<MessageBase> msgList = new List<MessageBase>();
            msgList.Add(canMsg);
            //msgList.Add(canFDMsg);


            BinlogReadWrite.writeBLF("D:\\tzhs\\testWrite.blf", msgList);
            List<MessageBase>  messageList = BinlogReadWrite.readBLF("D:\\tzhs\\testWrite.blf");

        }



    }



}