using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VectorBLFTools
{

    public enum GENERIC : uint {
        GENERIC_READ = 0x80000000,
        GENERIC_WRITE = 0x40000000
    }

    public enum BLFObjectType : uint{
        BL_OBJ_TYPE_CAN_MESSAGE = 1,
        BL_OBJ_TYPE_SYS_VARIABLE = 72,
        BL_OBJ_TYPE_APP_TEXT = 65,
        BL_OBJ_TYPE_CAN_FD_MESSAGE = 100,
        BL_OBJ_TYPE_CAN_FD_MESSAGE_64 = 101,
        BL_OBJ_TYPE_WATER_MARK_EVENT = 127
    }

    public enum BLAppID {
        BL_APPID_UNKNOWN = 0,
        BL_APPID_CANALYZER = 1,
        BL_APPID_CANOE = 2,
        BL_APPID_CANSTRESS = 3,
        BL_APPID_CANLOG = 4,
        BL_APPID_CANAPE = 5,
        BL_APPID_CANCASEXLLOG = 6,
        BL_APPID_VLCONFIG = 7,
        BL_APPID_PORSCHELOGGER = 200,
        BL_APPID_CAETECLOGGER = 201,
        BL_APPID_VECTORNETWORKSIMULATOR = 202,
        BL_APPID_IPETRONIKLOGGER = 203,
        BL_APPID_RT_RK = 204,
        BL_APPID_PIKETEC = 205,
        BL_APPID_SPARKS = 206,
        BL_APPID_TOYOTA = 207,
        BL_APPID_GEELY = 208,
        BL_APPID_ESR = 209,
        BL_APPID_VIGEM = 210,
        BL_APPID_X2E = 211,
        BL_APPID_CONTINENTAL = 212,
        BL_APPID_HYUNDAI_KIA = 213,
        BL_APPID_IAV = 214,
        BL_APPID_DAIMLERTRUCK = 215,
        BL_APPID_MINEBEAMITSUMI = 216,
        BL_APPID_MERCEDESBENZ = 217,
        BL_APPID_MONARCH_TRACTOR = 218,
        BL_APPID_TTTECH = 219
    }


    public enum ObjectFlag {

        BL_OBJ_FLAG_TIME_TEN_MICS = 0x00000001,    //10 micro second timestamp
        BL_OBJ_FLAG_TIME_ONE_NANS = 0x00000002    //1 nano second timestamp
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]

    public struct VBLFileStatistics
    {
        // sizeof (VBLFileStatistics)
        public UInt32 mStatisticsSize;

        // application ID
        public Byte mApplicationID;

        // application major number (Version)
        public Byte mApplicationMajor;

        // application minor number  (Version)
        public Byte mApplicationMinor;

        // application build number  (Version)
        public Byte mApplicationBuild;

        // file size in bytes
        public UInt64 mFileSize; 

        // uncompressed file size in bytes
        public UInt64 mUncompressedFileSize;

        // number of objects
        public UInt32 mObjectCount;

        // number of objects read
        public UInt32 mObjectsRead;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct VBLObjectHeaderBase
    {
        // signature (BL_OBJ_SIGNATURE)
        public UInt32 mSignature;     

        // sizeof object header
        public UInt16 mHeaderSize;   

        // header version
        public UInt16 mHeaderVersion; 

        // object size
        public UInt32 mObjectSize;    

        // object type 
        public UInt32 mObjectType;  
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)] // Common packing for structs with UInt64
    public struct VBLObjectHeader
    {
        // Nested structure: base header object
        public VBLObjectHeaderBase mBase; 

        // object flags
        public UInt32 mObjectFlags; 

        // client index of send node
        public UInt16 mClientIndex; 

        // object specific version
        public UInt16 mObjectVersion; 

        // object timestamp
        public UInt64 mObjectTimeStamp;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct VBLCANMessage
    {
        // object header (nested structure)
        public VBLObjectHeader mHeader; 
        // application channel
        public UInt16 mChannel;
        // CAN dir & rtr
        public Byte mFlags;
        // CAN dlc
        public Byte mDLC; 
        // CAN ID
        public UInt32 mID;
        // CAN data (fixed-size array)
        // Use [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] for fixed-size arrays.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public Byte[] mData; // Corresponds to ctypes.c_uint8*8
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct VBLCANFDExtFrameData
    {
        public UInt32 mBTRExtArb;  // Corresponds to ctypes.c_uint32
        public UInt32 mBTRExtData; // Corresponds to ctypes.c_uint32
    }

    public struct VBLCANFDMessage64
    {
        public VBLObjectHeader mHeader;        // object header

        public Byte mChannel;          // application channel
        public Byte mDLC;              // CAN dlc
        public Byte mValidDataBytes;   // Valid payload length of mData
        public Byte mTxCount;          // TXRequiredCount (4 bits), TxReqCount (4 Bits)

        public UInt32 mID;             // CAN ID
        public UInt32 mFrameLength;    // message length in ns - without 3 inter frame space bits
        public UInt32 mFlags;          // flags
        public UInt32 mBtrCfgArb;      // bit rate used in arbitration phase
        public UInt32 mBtrCfgData;     // bit rate used in data phase
        public UInt32 mTimeOffsetBrsNs;    // time offset of brs field
        public UInt32 mTimeOffsetCrcDelNs; // time offset of crc delimiter field

        public UInt16 mBitCount;       // complete message length in bits
        public Byte mDir;
        public Byte mExtDataOffset;

        public UInt32 mCRC;            // CRC for CAN

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public Byte[] mData;          // CAN FD data

        public VBLCANFDExtFrameData mExtFrameData; // Nested extended frame data
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct VBLCANFDMessage
    {
        public VBLObjectHeader mHeader;         // object header

        public UInt16 mChannel;                 // application channel
        public Byte mFlags;                     // CAN dir & rtr
        public Byte mDLC;                       // CAN dlc
        public UInt32 mID;                      // CAN ID
        public UInt32 mFrameLength;             // message length in ns - without 3 inter frame space bits and by Rx-message also without 1 End-Of-Frame bit
        public Byte mArbBitCount;               // bit count of arbitration phase
        public Byte mCANFDFlags;                // CAN FD flags
        public Byte mValidDataBytes;            // Valid payload length of mData
        public Byte mReserved1;                 // reserved
        public UInt32 mReserved2;               // reserved

        // CAN FD data (fixed-size array)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public Byte[] mData;                    // Corresponds to ctypes.c_uint8*64
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        public UInt16 wYear;         // Current year (e.g., 2025)
        public UInt16 wMonth;        // Current month (1-12)
        public UInt16 wDayOfWeek;    // Day of the week (0=Sunday, 1=Monday, ..., 6=Saturday)
        public UInt16 wDay;          // Day of the month (1-31)
        public UInt16 wHour;         // Hour (0-23)
        public UInt16 wMinute;       // Minute (0-59)
        public UInt16 wSecond;       // Second (0-59)
        public UInt16 wMilliseconds; // Milliseconds (0-999)
    }


    


    public class BLFAPI
    {

        public static ulong NULLPtr = 0xFFFFFFFFFFFFFFFF;

        public static uint BL_OBJ_SIGNATURE = 0x4A424F4C;

        [DllImport(".\\binlog.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr BLCreateFileW(string filePath, GENERIC genricFlag);

        [DllImport(".\\binlog.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint BLCloseHandle(IntPtr filePtr);


        [DllImport(".\\binlog.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int BLGetFileStatistics(IntPtr filePtr, out VBLFileStatistics stats);

        [DllImport(".\\binlog.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int BLPeekObject(IntPtr filePtr, out VBLObjectHeaderBase fileHeader);

        [DllImport(".\\binlog.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int BLReadObjectSecure(IntPtr filePtr, IntPtr vBLObjectHeaderBase, UIntPtr size);


        [DllImport(".\\binlog.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int BLSetApplication(IntPtr filePtr, BLAppID appID,byte applicationMajor,byte applicationMinor,byte applicationBuild);

        [DllImport(".\\binlog.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int BLSetMeasurementStartTime(IntPtr filePtr, IntPtr timePtr );

        [DllImport(".\\binlog.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int BLSetWriteOptions(IntPtr filePtr, uint level_0,uint level_1);


        [DllImport(".\\binlog.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int BLWriteObject(IntPtr filePtr, IntPtr msgPtr);

    }
}
