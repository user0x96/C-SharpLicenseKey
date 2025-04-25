//#Copyright 2025 © @user0x96
//#Github: https://github.com/user0x96
//#Donate⭐ 
//#+USDT(TRC-20): TCLCdvvvgy6Pbj5VnTzaYBwGKzBDyBEyGL
//#+BTC: 3EALRZzA5vp7i6kXhELTujphiJC4WwZESF
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckKeyV2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CheckKeyV2VIP(); //Call the function to check the key
        }
        //Creat CMD
        static string RunCMD(string cmd)
        {
            Process cmdProcess = new Process();
            cmdProcess.StartInfo.FileName = "cmd.exe";
            cmdProcess.StartInfo.Arguments = "/c " + cmd;
            cmdProcess.StartInfo.RedirectStandardOutput = true;
            cmdProcess.StartInfo.UseShellExecute = false;
            cmdProcess.StartInfo.CreateNoWindow = true;
            cmdProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmdProcess.Start();
            string output = cmdProcess.StandardOutput.ReadToEnd();
            cmdProcess.WaitForExit();
            return output;
        }

        //Creat CheckKey
        private static void CheckKeyV2VIP()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;

                //Get Serial Number of Hdd
                string hddSerial = RunCMD("wmic diskdrive get serialNumber");
                string[] hddLines = hddSerial.Split('\n');
                if (hddLines.Length < 2) throw new Exception("Failed to retrieve HDD Serial Number.");
                string cleanedHddSerial = Regex.Replace(hddLines[1].Trim(), "\\s+", "");

                //Get Serial Number of BIOS
                string biosSerial = RunCMD("wmic bios get serialnumber");
                string[] biosLines = biosSerial.Split('\n');
                if (biosLines.Length < 2) throw new Exception("Failed to retrieve BIOS Serial Number.");
                string cleanedBiosSerial = Regex.Replace(biosLines[1].Trim(), "\\s+", "");

                //Combine both Serial + BIOS
                string key = cleanedBiosSerial + cleanedHddSerial;

                //Encypt MD5 key
                MD5 md5 = MD5.Create();
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
                string md5Hash = BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();

                //Contact Buy
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Your Key is: " + md5Hash);
                Console.WriteLine("Telegram: @user0x96");

                //Login information
                Console.Write("Enter Account: ");
                string account = Console.ReadLine();
                Console.Write("Enter Password: ");
                string password = Console.ReadLine();

                //Send HTTP request to Google Sheets
                HttpClient httpClient = new HttpClient();
                string requestUri = "#=================== Enter your spreadsheet link here ==================="; //"#=================== <<<<<<Enter your spreadsheet link here>>>>>> ==================="
                string responseContent = httpClient.GetAsync(requestUri).Result.Content.ReadAsStringAsync().Result;

                //Regex pattern to find key
                string pattern = md5Hash + ".*?(?=OK)";
                Match match = Regex.Match(responseContent, pattern);

                if (match.Success)
                {
                    string[] matchDetails = match.Value.Split('|');
                    if (matchDetails.Length < 4) throw new Exception("Data from Google Sheets is not in the correct format.");

                    string expiryDateString = matchDetails[1];
                    string accountFromSheet = matchDetails[2];
                    string passwordFromSheet = matchDetails[3];

                    if (expiryDateString.Contains("LifeTime"))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Login Successfully!\nRemaining: LifeTime!\nAccount: {account}\nPassword: {password}");
                    }
                    else
                    {
                        //Handle expiration date
                        string[] dateParts = expiryDateString.Split('/');
                        if (dateParts.Length != 3) throw new Exception("Invalid expiry date format.");
                        DateTime expiryDate = new DateTime(int.Parse(dateParts[2]), int.Parse(dateParts[1]), int.Parse(dateParts[0]));
                        DateTime currentDate = DateTime.Now;

                        int daysRemaining = (int)(expiryDate - currentDate).TotalDays;

                        if (daysRemaining <= 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Software expired! Please contact Admin to renew.");
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            string dayText = (daysRemaining == 1) ? "day" : "days";
                            Console.WriteLine($"Login Successfully!\nRemaining: {daysRemaining} {dayText}");
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid key! Please contact Admin to purchase key.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
    }
}