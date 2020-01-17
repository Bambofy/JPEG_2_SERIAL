
 # JPEG 2 SERIAL
 
 JPG 2 SERIAL is a cross-platform command line tool built with .NET Core for uploading an image, or multiple images, over
 a serial connection to an awaiting device.
 
 ## Notes
 
 This tool is case sensitive, therefore, None must have a capital N.etc.
 
 ## Usage
 
 With the JPG_2_SERIAL.exe in the same folder as FILENAME.JPG and a sub-directory called IMAGES/ which contains multiple images.
 
 ./JPG_2_SERIAL.exe -input=FILENAME.JPG -mode=single -baud_rate=9600 -port_name=COM4
 
 ./JPG_2_SERIAL.exe -input=DIRECTORY/ -mode=sequence -baud_rate=9600 -port_name=COM4
 
 ## Available Arguments
 |Argument Name|Valid Input|Optional?|Description|
|--|--|--|--|
|-input||NO|Set the target file or directory (see the mode argument).|
|-buffer_size|Default: 1024|YES|Sets the number of byte wrote in one buffer.|
|-delay_time|Default: 10ms|YES|Sets the amount of time between each buffer write.|
|-mode|single,sequence|NO|Single file upload or upload a directory or images?|
|-baud_rate||NO|Set the baud rate for the serial connection.|
|-port_name||NO|Set the serial port object's target port's name.|                                      
|-parity|None,Odd,Even,Mark,Space|NO|Set the parity of the serial connection.|                                 
|-data_bits||YES|Set the data bits|
|-stop_bits|None,One,OnePointFive,Two|YES|Set the stop bits|
|-handshake|None,XOnXOff,RequestToSend,RequestToSendXOnXOff|YES|Set the handshake|
 |-read_timeout|Default: 500|YES|Sets the read timeout (ms)|
|-write_timeout|Default: 500|YES|Sets the write timeout (ms)|

