using System;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

/// <summary>
/// # JPEG 2 SERIAL
/// 
/// JPG 2 SERIAL is a command line tool for uploading an image, or multiple images, over
/// a serial connection to an awaiting device.
/// 
/// ## Notes
/// 
/// This tool is case sensitive, therefore, None must have a capital N.etc.
/// 
/// ## Usage
/// 
/// With the JPG_2_SERIAL.exe in the same folder as FILENAME.JPG and a sub-directory called IMAGES/ which contains multiple images.
/// ./JPG_2_SERIAL.exe -input=FILENAME.JPG -mode=single -baud_rate=9600 -port_name=COM4
/// ./JPG_2_SERIAL.exe -input=DIRECTORY/ -mode=sequence -baud_rate=9600 -port_name=COM4
/// 
/// ## Available Arguments
/// 
///	|	Argument Name	|					Valid Input							|	Optional?	|	Description													|
///	|-------------------|-------------------------------------------------------|---------------|---------------------------------------------------------------|
///	|	-input			|														|	NO			|	Set the target file or directory (see the mode argument).	|
///	|	-buffer_size	|	Default: 1024										|	YES			|	Sets the number of byte wrote in one buffer.				|
///	|	-delay_time		|	Default: 10ms										|	YES			|	Sets the amount of time between each buffer write.			|
///	|	-mode			|	single,sequence										|	NO			|	Single file upload or upload a directory or images?			|
///	|	-baud_rate		|														|	NO			|	Set the baud rate for the serial connection.				|
///	|	-port_name		|														|	NO			|	Set the serial port object's target port's name.			|                                      
///	|	-parity			|	None,Odd,Even,Mark,Space							|	NO			|	Set the parity of the serial connection.					|                                 
///	|	-data_bits		|														|	YES			|	Set the data bits											|
///	|	-stop_bits		|	None,One,OnePointFive,Two							|	YES			|	Set the stop bits											|
///	|	-handshake		|	None,XOnXOff,RequestToSend,RequestToSendXOnXOff		|	YES			|	Set the handshake											|
/// |	-read_timeout	|	Default: 500										|	YES			|	Sets the read timeout (ms)									|
///	|	-write_timeout	|	Default: 500										|	YES			|	Sets the write timeout (ms)									|
///	
/// </summary>
/// 

namespace JPG_2_SERIAL
{
	class Program
	{

		static void Main(string[] args)
		{
			// default settings.
			Dictionary<String, String> configurationSettings = new Dictionary<string, string>();
			configurationSettings.Add("INPUT", "");
			configurationSettings.Add("BUFFER_SIZE", "1024");
			configurationSettings.Add("DELAY_SIZE", "10");
			configurationSettings.Add("MODE", "single");
			configurationSettings.Add("BAUD_RATE", "9600");
			configurationSettings.Add("PORT_NAME", "");
			configurationSettings.Add("PARITY", "");
			configurationSettings.Add("DATA_BITS", "8");
			configurationSettings.Add("STOP_BITS", "");
			configurationSettings.Add("HANDSHAKE", "");
			configurationSettings.Add("READ_TIMEOUT", "500");
			configurationSettings.Add("WRITE_TIMEOUT", "500");


			// find each command in the list of argument given.
			foreach (string arg in args)
			{
				// ensure each argument has a value.
				if (!arg.Contains("="))
				{
					Console.WriteLine("Argument given that does not use the format -ARG=VALUE" + arg);
					Console.ReadLine();
					return;
				}

				// split the argument into name and value.
				string[] argumentParts = arg.Split("=");
				if (argumentParts.Length > 2)
				{
					Console.WriteLine("Argument given that has too many VALUEs: " + arg);
					Console.ReadLine();
					return;
				}
				string argumentName = argumentParts[0];
				string argumentValue = argumentParts[1];



				// convert this input and compare it to the default configuration.
				string uppercaseArgName = argumentName.ToUpper();

				// if it exists in the settings dictionary, overwrite the key's value.
				if (configurationSettings.ContainsKey(uppercaseArgName))
				{
					configurationSettings[uppercaseArgName] = argumentValue;
				}
				else
				{
					Console.WriteLine("Argument given that does not exist: " + arg);
					Console.ReadLine();
					return;
				}
			}

			// now the configuration will be updated with given input.
			ImageToPortHandler imageToPortHandler = new ImageToPortHandler();

			imageToPortHandler.Send(configurationSettings);			
		}
	}

	public class ImageToPortHandler
	{
		private SerialPort _serialPort;
		private Dictionary<string, string> _configurationSettings;

		/// <summary>
		/// Configures the serial port to the given settings.
		/// </summary>
		/// <param name="pConfigurationSettings"></param>
		public void Send(Dictionary<string, string> pConfigurationSettings)
		{
			_configurationSettings = pConfigurationSettings;



			Console.WriteLine("Configuring serial port...");

			_serialPort = new SerialPort();

			_serialPort.PortName = pConfigurationSettings["PORT_NAME"];
			_serialPort.BaudRate = Convert.ToInt32(pConfigurationSettings["BAUD_RATE"]);

			Parity parity;
			Enum.TryParse<Parity>(pConfigurationSettings["PARITY"], out parity);
			_serialPort.Parity = parity;

			_serialPort.DataBits = Convert.ToInt32(pConfigurationSettings["DATA_BITS"]);

			StopBits stopBits;
			Enum.TryParse<StopBits>(pConfigurationSettings["STOP_BITS"], out stopBits);
			_serialPort.StopBits = stopBits;

			Handshake handshake;
			Enum.TryParse<Handshake>(pConfigurationSettings["HANDSHAKE"], out handshake);
			_serialPort.Handshake = handshake;

			_serialPort.ReadTimeout = Convert.ToInt32(pConfigurationSettings["READ_TIMEOUT"]);
			_serialPort.WriteTimeout = Convert.ToInt32(pConfigurationSettings["WRITE_TIMEOUT"]);

			Console.WriteLine("Done!");



			if (_configurationSettings["MODE"] == "single")
			{
				this.SendImage();
			}
			else if (_configurationSettings["MODE"] == "sequence")
			{
				this.SendImageSequence();
			}
		}


		private void SendImage()
		{
			Console.WriteLine("Sending individual image: " + _configurationSettings["INPUT"]);

			// stream the image byte by byte to the serial port.

			string filePath = Path.Combine(Directory.GetCurrentDirectory(), _configurationSettings["INPUT"]);

			// create a buffer to make sure the serial port doesn't get overwhelmed.
			int bufferSize = Convert.ToInt32(_configurationSettings["BUFFER_SIZE"]);
			byte[] buffer = new byte[bufferSize];

			// number of milliseconds between each buffer write so the serial port doesn't get overwhelmed.
			int restTime = Convert.ToInt32(_configurationSettings["DELAY_TIME"]);

			using (FileStream imageFileStream = File.OpenRead(filePath))
			{
				// open serial port.
				_serialPort.Open();

				// read buffersize into buffer.
				int bytesReadFromFile = imageFileStream.Read(buffer, 0, bufferSize);
				// write buffer to serial port.
				_serialPort.Write(buffer, 0, bytesReadFromFile);

				while (bytesReadFromFile > 0)
				{
					// read buffersize into buffer.
					bytesReadFromFile = imageFileStream.Read(buffer, 0, bufferSize);
					// write buffer to serial port.
					_serialPort.Write(buffer, 0, bytesReadFromFile);

					// stop when we reach the end of the file.
					if (bytesReadFromFile == 0)
					{
						break;
					}

					// wait for a bit incase serial port is slow.
					if (restTime > 0)
					{
						Thread.Sleep(restTime);
					}
				}


				_serialPort.Close();
			}

			Console.WriteLine("Done!");
		}

		private void SendImageSequence()
		{
			Console.WriteLine("Sending sequence of images from directory: " + _configurationSettings["INPUT"]);

			// stream the image byte by byte to the serial port.

			string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), _configurationSettings["INPUT"]);

			// create a buffer to make sure the serial port doesn't get overwhelmed.
			int bufferSize = Convert.ToInt32(_configurationSettings["BUFFER_SIZE"]);
			byte[] buffer = new byte[bufferSize];

			// number of milliseconds between each buffer write so the serial port doesn't get overwhelmed.
			int restTime = Convert.ToInt32(_configurationSettings["DELAY_TIME"]);

			foreach (string filePathsInDirectory in Directory.GetFiles(directoryPath))
			{
				// open serial port.
				_serialPort.Open();

				using (FileStream imageFileStream = File.OpenRead(filePathsInDirectory))
				{
					// read buffersize into buffer.
					int bytesReadFromFile = imageFileStream.Read(buffer, 0, bufferSize);
					// write buffer to serial port.
					_serialPort.Write(buffer, 0, bytesReadFromFile);

					while (bytesReadFromFile > 0)
					{
						// read buffersize into buffer.
						bytesReadFromFile = imageFileStream.Read(buffer, 0, bufferSize);
						// write buffer to serial port.
						_serialPort.Write(buffer, 0, bytesReadFromFile);

						// stop when we reach the end of the file.
						if (bytesReadFromFile == 0)
						{
							break;
						}

						// wait for a bit incase serial port is slow.
						if (restTime > 0)
						{
							Thread.Sleep(restTime);
						}
					}
				}

				// close the serial port.
				_serialPort.Close();
			}


			Console.WriteLine("Done!");
		}

		~ImageToPortHandler()
		{
		}
	}
}
