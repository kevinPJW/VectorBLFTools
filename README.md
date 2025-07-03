
# VectorBLFTools

This is a tool library for reading and writing Vector can.blf files. This library is written in C#. Currently, this library only reads and writes to ordinary can and canfd messages. Extended frames, remote frames, and error frames are not supported yet. To be improved later.


## License

[MIT](https://choosealicense.com/licenses/mit/)


## Appendix

I searched for a long time and found that C# doesn't have a library to implement reading blf files. So I implemented it and made it open source.

This code is implemented based on the binlog.dll dynamic link library provided by Vector officially. When you use my library, be sure to place **binlog.dll** in the same directory as the executable file.


**.net framework >= 4.7.2**
## Usage/Examples

write blf
```C#

List<MessageBase> msgList = new List<MessageBase>();

CANMessage canMsg = new CANMessage(1,0x10,new byte[] {0x00,0x11,0x22,0x33,0x44,0x55,0x66,0x77 },20);
CANFDMessage canFDMsg = new CANFDMessage(2,0x57F,0,new byte[16] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 , 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 },0.2);

msgList.Add(canMsg);
msgList.Add(canFDMsg);

BinlogReadWrite.writeBLF("D:\\tzhs\\testWrite.blf", msgList);
//Call this function will throw an exception, which needs to be caught and handled
```

read blf
```C#
List<MessageBase>  messageList = BinlogReadWrite.readBLF("D:\\tzhs\\testWrite.blf");
//Call this function will throw an exception, which needs to be caught and handled
```
## Authors

- [@KevinPJW](https://github.com/kevinPJW)
