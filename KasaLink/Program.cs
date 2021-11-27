using Microsoft.Extensions.Configuration;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;

namespace KasaLink
{
    class Program
    {
        static int Main(string[] args)
        {
            var cmd = new RootCommand
            {
                new Argument<string>("device", "Kasa device name."),
                new Argument<string>("command", "Command to run."),
                new Option(new[] { "--verbose", "-v" }, "Show device messages.")
            };

            cmd.Handler = CommandHandler.Create<string, string, bool>((device, command, verbose) =>
            {
                try
                {
                    var config = new ConfigurationBuilder()
                      .AddIniFile("config.ini")
                      .Build();

                    var host = config["Devices:" + device];
                    var cmnd = config["Commands:" + command];

                    if (host == null || cmnd == null)
                        throw new Exception("Config entry missing");

                    if (verbose)
                        Console.WriteLine(cmnd);
                    var msg = SendCommand(host, cmnd);
                    if (verbose)
                        Console.WriteLine(msg);

                    var root = JsonDocument.Parse(msg).RootElement;
                    var first = root.EnumerateObject().First().Value;

                    if (first.TryGetProperty("err_code", out var code))
                        return code.GetInt32();
                    else
                        return first.EnumerateObject().First().Value.GetProperty("err_code").GetInt32();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return -1;
                }
            });

            return cmd.Invoke(args);
        }

        public static string SendCommand(string host, string msg)
        {
            string result;

            using (var client = new TcpClient())
            {
                client.Connect(host, 9999);

                using (var stream = client.GetStream())
                {
                    var encrypted = Encrypt(msg);
                    stream.Write(encrypted, 0, encrypted.Length);

                    var lenBuffer = new byte[4];
                    stream.Read(lenBuffer, 0, lenBuffer.Length);
                    var len = BinaryPrimitives.ReadInt32BigEndian(lenBuffer);

                    var readBuffer = new byte[len];

                    var bytesRead = 0;
                    while (bytesRead < len)
                        bytesRead += stream.Read(readBuffer, bytesRead, len - bytesRead);

                    result = Decrypt(readBuffer);
                }
            }

            return result;
        }

        private static byte[] Encrypt(string source)
        {
            var arr = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(arr, source.Length);
            var bytes = new List<byte>(arr);

            int key = 171;
            for (int i = 0; i < source.Length; i++)
            {
                var a = key ^ source[i];
                key = a;
                bytes.Add((byte)a);
            }

            return bytes.ToArray();
        }

        private static string Decrypt(byte[] source)
        {
            var str = String.Empty;

            int key = 171;
            for (int i = 0; i < source.Length; i++)
            {
                var a = key ^ source[i];
                key = source[i];
                str += (char)a;
            }

            return str;
        }
    }
}