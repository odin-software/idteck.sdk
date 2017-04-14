using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using IDTECK.SDK.BaseLib;
using IDTECK.SDK.MAC1000.Api;
using IDTECK.SDK.MAC1000.Type;

namespace polling_test
{
    class Program
    {
        private static CCommon __ccommon = new CCommon();
        private static iMACAPI __cmacapi = new iMACAPI();

        static void Main(string[] args)
        {
            while (true)
            {
                if (Console.KeyAvailable == true)
                    if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                        break;

                var _recv_type18 = __cmacapi.BroadcastDownloadToController(p_board_id: 1);
                {
                    if (_recv_type18.packet.recv_packet != null)
                        Console.WriteLine(__ccommon.ByteToHexString(_recv_type18.packet.recv_packet));

                    if (_recv_type18.is_success == true)
                    {
                        Function_Pollings(_recv_type18);
                        break;
                    }

                    Thread.Sleep(5000);
                }
            }
        }

        public static void Function_Pollings(RecvType18 p_recv_type18)
        {
            var _tcp_client = (TcpClient)null;

            while (true)
            {
                try
                {
                    if (Console.KeyAvailable == true)
                        if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                            break;

                    if (_tcp_client == null)
                        _tcp_client = __cmacapi.OpenPolling(p_recv_type18.ip_address, p_recv_type18.port);

                    var _message = __cmacapi.RecvPolling(p_recv_type18.board_id, _tcp_client, true);
                    if (_message.is_success == false)
                    {
                        WriteFileLog(String.Format("send: {0}, recv-msg: {1}", __ccommon.ByteToHexString(_message.packet.send_packet), _message.error_message));

                        __cmacapi.ClosePolling(_tcp_client);
                        _tcp_client = null;

                        Thread.Sleep(1000);
                    }
                    else
                    {
                        if (_message.has_polling_event == true)
                            WriteFileLog(String.Format("send: {0}, recv-evt: {1}", __ccommon.ByteToHexString(_message.packet.send_packet), __ccommon.ByteToHexString(_message.packet.recv_packet)));
                        else
                            WriteConsoleLog(String.Format("send: {0}, recv-dat: {1}", __ccommon.ByteToHexString(_message.packet.send_packet), __ccommon.ByteToHexString(_message.packet.recv_packet)));
                    }
                }
                catch (Exception ex)
                {
                    WriteFileLog(ex.Message);

                    if (_tcp_client != null)
                    {
                        __cmacapi.ClosePolling(_tcp_client);
                        _tcp_client = null;
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private static void WriteConsoleLog(string message)
        {
            Console.WriteLine(String.Format("[{0}]: {1}\r\n", CExtend.ToLogDateTimeString(), message));
        }

        private static void WriteFileLog(string message)
        {
            var _log_folder = Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                                , "IDTECK"
                            );

            if (Directory.Exists(_log_folder) == false)
                Directory.CreateDirectory(_log_folder);

            var _log_file = Path.Combine(_log_folder, "log_polling_" + DateTime.Now.ToShortDateString() + ".log");
            File.AppendAllText(_log_file, String.Format("[{0}]: {1}\r\n", CExtend.ToLogDateTimeString(), message));

            WriteConsoleLog(message);
        }
    }
}