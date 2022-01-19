using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientUsingTcp
{
    class Program
    {
        static void Main(string[] args)
        {
            //show usage if not given a single argument
            if (args.Length != 1)
            {
                ShowUsage();
                return;
            }
            
            //send out an HTTP request over TCP and wait
            Task<string> t1 = RequestHtmlAsync(args[0]);
            
            //write out the result returned from the send request
            Console.WriteLine(t1.Result);
            Console.ReadLine();
        }

        private static void ShowUsage() =>
            Console.WriteLine("Usage: HttpClientUsingTcp hostname");

        private const int ReadBufferSize = 1024; 
        //used to encode the network bit stream into a string
        public static async Task<string> RequestHtmlAsync(string hostname)
        {
            try
            {
                using (var client = new TcpClient())  //create a TCP client object
                {
                    //connect to port 80 of the host provided and wait
                    await client.ConnectAsync(hostname, 80);
                   
                    //create a network stream and use it send a request to the host
                    //convert the request string into a bitstream into a write buffer
                    //flush the stream to send all the bytes to the host
                    NetworkStream stream = client.GetStream();
                    string header = "GET / HTTP/1.1\r\n" +
                        $"Host: {hostname}:80\r\n" +
                        "Connection: close\r\n" +
                        "\r\n";
                    byte[] buffer = Encoding.UTF8.GetBytes(header);
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    await stream.FlushAsync();

                    //create a memory stream and a read buffer
                    var ms = new MemoryStream();
                    buffer = new byte[ReadBufferSize];
                    int read = 0;
                    
                    
                    //while there bytes to be read off the network stream
                    //read the bytes into a read buffer
                    //write the buffer to the memory stream
                    //clear down the buffer
                    do
                    {
                        read = await stream.ReadAsync(buffer, 0, ReadBufferSize);
                        ms.Write(buffer, 0, read);
                        Array.Clear(buffer, 0, buffer.Length);
                    } while (read > 0);

                    //from the begining of the memory stream
                    //use a stream reader object to return 
                    //the full response back to the calling method
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(ms))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (SocketException ex)
            {
                //write an error message to the console
                //if the their is an exception
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
