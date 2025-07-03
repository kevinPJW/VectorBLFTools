using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VectorBLFTools
{
    public enum CANType { 
        CAN,
        CANFD
    }

    public enum MessageFlag
    {
        MSG_STD = (byte)0x01,   
        MSG_EXT = (byte)0x02,   
        MSG_RTR = (byte)0x04,   
        MSG_ERROR_FRAME = (byte)0x08    
    }


    public abstract class MessageBase {

        public CANType canType;
        public MessageFlag messageFlag;

        public MessageBase(CANType canType_, MessageFlag messageFlag_) {
            this.canType = canType_;
            this.messageFlag = messageFlag_;
        }
    }


    public class CANMessage : MessageBase
    {
        public uint channel;
        public byte flags;
        public byte DLC;
        public uint ID;
        public byte[] data;
        public double timeStamp; //msec

        public CANMessage(uint channle_ ,uint ID_, byte[]data_, double timeStamp_, MessageFlag messageFlag_ = MessageFlag.MSG_STD) : base(CANType.CAN, messageFlag_) { 
            this.channel = channle_;
            this.DLC = 8;
            this.ID = ID_;
            this.data = data_;
            this.timeStamp = timeStamp_;
        }

        public override string ToString()
        {
            string idHex = $"0x{ID:X}";

            string dataHex = (data != null && data.Length > 0)
                ? string.Join(" ", data.Select(b => b.ToString("X2"))) 
                : "[]"; 

            return $"CANMessage: " +
                   $"canType={base.canType.ToString()}, "+
                   $"Channel={channel}, " +
                   $"DLC={DLC}, " +
                   $"ID={idHex}, " +
                   $"Data=[{dataHex}], " +
                   $"Timestamp={timeStamp} ms";
        }
    }


    public class CANFDMessage : MessageBase{

        public uint channel;
        public uint ID;
        public byte[] data;
        public double timeStamp;//msec



        public CANFDMessage(uint channel_,uint ID_
            ,byte dir_, byte[]data_, double timeStamp_, MessageFlag messageFlag_ = MessageFlag.MSG_STD) : base(CANType.CANFD, messageFlag_)
        {
            this.channel = channel_;
            this.ID = ID_;
            this.data = data_;
            this.timeStamp = timeStamp_;
        }
        public override string ToString()
        {
            string idHex = $"0x{ID:X}";

            string dataHex = (data != null && data.Length > 0)
                ? string.Join(" ", data.Select(b => b.ToString("X2"))) 
                : "[]"; 

            return $"CANMessage: " +
                   $"canType={base.canType.ToString()}, " +
                   $"Channel={channel}, " +
                   $"ID={idHex}, " +
                   $"Data=[{dataHex}], " +
                   $"Timestamp={timeStamp} ms";
        }
    }
}
