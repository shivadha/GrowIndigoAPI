using GrowIndigo.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace GrowIndigo.Common
{
    public class Authentication
    {

        public static string gettoken(HttpRequest req)
        {
            var token = req.Headers.GetValues("token").FirstOrDefault();

            return "";
        }

        //public string createtoken(string mobileno)
        //{
        //    string encryptedstring = "";
        //    using (GrowIndigoAPIDBEntities dbContext = new GrowIndigoAPIDBEntities())
        //    {
        //        var Otp = dbContext.UserOTPInfo.Where(x => x.MobileNumber == mobileno).OrderBy(x => x.GenratedDate).Select(x => x.OTP).FirstOrDefault();
        //        if (Otp != "")
        //        {
        //            var token = mobileno + "/" + Otp;
        //            encryptedstring = encrypt(token);
        //        }

        //        return encryptedstring;
        //    }
        //}


        public bool verifytoken(string token)
        {
            string decryptedtoken = "";
            try
            {
                using (GrowIndigoAPIDBEntities dbContext = new GrowIndigoAPIDBEntities())
                {
                    decryptedtoken = Decrypt(token);
                    var res = decryptedtoken.Split('/');

                    if (res != null)
                    {
                        string mobileno = res[0] != null ? res[0] : "";
                        string otp = res[1] != null ? res[1] : "";
                        var user = dbContext.UserOTPInfo.Where(x => x.MobileNumber == mobileno && x.OTP == otp).FirstOrDefault();

                        if (user != null)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }


        public string encrypt(string encryptString)
        {
            try
            {
                string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        encryptString = Convert.ToBase64String(ms.ToArray());
                    }
                }
                return encryptString;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                cipherText = cipherText.Replace(" ", "+");
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                return cipherText;
            }
            catch (Exception ex)
            {
                return "";
            }
        }


    }
}