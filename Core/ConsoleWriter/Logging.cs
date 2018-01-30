using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

using Plus;

namespace ConsoleWriter
{
    public class Writer
    {
        private static bool mDisabled;

        public static bool DisabledState
        {
            get { return mDisabled; }
            set { mDisabled = value; }
        }

        public static void WriteLine(string Line, ConsoleColor Colour = ConsoleColor.Gray)
        {
            if (!mDisabled)
            {
                Console.ForegroundColor = Colour;
                Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + Line);
            }
        }

        public static void WriteProductData(string ProductData)
        {
            WriteToFile(@"productdata.txt", ProductData);
        }

        public static void LogException(string logText)
        {
            WriteToFile(@"Logs\exceptions.txt", logText + "\r\n\r\n");
            //WriteLine("Exception has been saved", ConsoleColor.Red);
        }

        public static void LogCriticalException(string logText)
        {
            WriteToFile(@"Logs\criticalexceptions.txt", logText + "\r\n\r\n");
            //WriteLine("CRITICAL ERROR LOGGED", ConsoleColor.Red);
        }

        public static void LogMySQLError(string logText)
        {
            WriteToFile(@"Logs\mysql_error.txt", logText + "\r\n\r\n");
            //WriteLine("CRITICAL ERROR LOGGED", ConsoleColor.Red);
        }

        public static void LogRPTimersError(string logText)
        {
            WriteToFile(@"Logs\rp_timers_errors.txt", logText + "\r\n\r\n");
            //WriteLine("CRITICAL ERROR LOGGED", ConsoleColor.Red);
        }

        public static void LogRPGamesError(string logText)
        {
            WriteToFile(@"Logs\rp_games_errors.txt", logText + "\r\n\r\n");
            //WriteLine("CRITICAL ERROR LOGGED", ConsoleColor.Red);
        }

        public static void LogRPBotError(string logText)
        {
            WriteToFile(@"Logs\rp_bots_errors.txt", logText + "\r\n\r\n");
            //WriteLine("CRITICAL ERROR LOGGED", ConsoleColor.Red);
        }

        public static void LogWebSocketError(string logText)
        {
            WriteToFile(@"Logs\rp_websocket_errors.txt", logText + "\r\n\r\n");
            //WriteLine("CRITICAL ERROR LOGGED", ConsoleColor.Red);
        }

        public static void LogWiredException(string logText)
        {
            WriteToFile(@"Logs\wiredexceptions.txt", logText + "\r\n\r\n");
        }

        public static void LogCacheException(string logText)
        {
            WriteToFile(@"Logs\cacheexceptions.txt", logText + "\r\n\r\n");
        }

        public static void LogPathfinderException(string logText)
        {
            WriteToFile(@"Logs\pathfinderexceptions.txt", logText + "\r\n\r\n");
        }

        public static void LogThreadException(string Exception, string Threadname)
        {
            WriteToFile(@"Logs\threaderror.txt", "Erro no thread " + Threadname + ": \r\n" + Exception + "\r\n\r\n");
            //WriteLine("Error in " + Threadname + " caught", ConsoleColor.Red);
        }

        public static void LogQueryError(Exception Exception, string query)
        {
            WriteToFile(@"Logs\MySQLerrors.txt", "Erro na query: \r\n" + query + "\r\n" + Exception + "\r\n\r\n");
            //WriteLine("Error in query caught", ConsoleColor.Red);
        }

        public static void LogPacketException(string Packet, string Exception)
        {
            WriteToFile(@"Logs\packeterror.txt", "Erro no pacote " + Packet + ": \r\n" + Exception + "\r\n\r\n");
            //WriteLine("Packet error!", ConsoleColor.Red);
        }

        public static void HandleException(Exception pException, string pLocation)
        {
            var ExceptionData = new StringBuilder();
            ExceptionData.AppendLine("Exceção registrada " + DateTime.Now.ToString() + " em " + pLocation + ":");
            ExceptionData.AppendLine(pException.ToString());
            if (pException.InnerException != null)
            {
                ExceptionData.AppendLine("Exceção interna:");
                ExceptionData.AppendLine(pException.InnerException.ToString());
            }
            if (pException.HelpLink != null)
            {
                ExceptionData.AppendLine("Link de ajuda:");
                ExceptionData.AppendLine(pException.HelpLink);
            }
            if (pException.Source != null)
            {
                ExceptionData.AppendLine("Fonte:");
                ExceptionData.AppendLine(pException.Source);
            }
            if (pException.Data != null)
            {
                ExceptionData.AppendLine("Dados:");
                foreach (DictionaryEntry Entry in pException.Data)
                {
                    ExceptionData.AppendLine("  Chave: " + Entry.Key + " Valor: " + Entry.Value);
                }
            }
            if (pException.Message != null)
            {
                ExceptionData.AppendLine("Mensagem:");
                ExceptionData.AppendLine(pException.Message);
            }
            if (pException.StackTrace != null)
            {
                ExceptionData.AppendLine("Traçado de trace:");
                ExceptionData.AppendLine(pException.StackTrace);
            }
            ExceptionData.AppendLine();
            ExceptionData.AppendLine();
            LogException(ExceptionData.ToString());
        }

        public static void DisablePrimaryWriting(bool ClearConsole)
        {
            mDisabled = true;
            if (ClearConsole)
                Console.Clear();
        }

        public static void WriteToFile(string path, string content)
        {
            try
            {
                FileStream Writer = new FileStream(path, FileMode.Append, FileAccess.Write);
                byte[] Msg = Encoding.ASCII.GetBytes(Environment.NewLine + content);
                Writer.Write(Msg, 0, Msg.Length);
                Writer.Dispose();
            }
            catch (Exception e)
            {
                WriteLine("Não foi possível escrever no arquivo: " + e + ":" + content);
            }
        }

        private static void WriteCallback(IAsyncResult callback)
        {
            var stream = (FileStream)callback.AsyncState;
            stream.EndWrite(callback);
            stream.Dispose();
        }
    }
}