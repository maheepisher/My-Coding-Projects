/* -----------------------------------------------------------------------
* Product/Project Name: CTS-Enhancement
* Module: CTS Integrator CTS-ITG-DBToFile
* File: GlobalClass.cs
* Purpose: Contains common functions



* * Functions Defined:
1) ReadConfig()
2) WriteLog()
3) ValidateConfig()
4) LogAPIAsync()
* /// <CHANGE HISTORY>
* ///
* ///-----------------------------------------------------------------------
* /// Date: Change By : Change Description



* /// </CHANGE HISTORY>
-----------------------------------------------------------------------*/


using CTSLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CTSIntegratorService
{
    public static class GlobalClass
    {
        public const string CTS_SUCCESS = "SUCCESS";
        public const string CTS_FAIL = "FAIL";
        public const string ServiceName = "CTSIntegrator";

        public static String sSource = "CTSIntegrator";
        public static String sLog = "CTSIntegrator";
        private static String sEvent;
        public static bool isStopCommandFired = false;

        public static XmlDocument xml_ConfigDoc;
        public static XmlNodeList xml_ConfigFileNodeList;

        public static Tag_DBEssential udtTag_DBEssential;
        public static Tag_CTSIntegrator udtTag_CTSIntegrator;
        public static Tag_CommonIImgDetails udtTag_CommonImgDetails;
        public static Tag_LogEssential udtTag_LogEssential;

        public static OmniDocs objOmniDocs = new OmniDocs();
        public static OmniFlow objOmniFlow = new OmniFlow();
        public static OmniFlow objOmniFlowDes = new OmniFlow();
        public static CTSWrapper.clsGlobalDecl.WMTSessionHandle oudtWMTSessionHandle = new CTSWrapper.clsGlobalDecl.WMTSessionHandle();


        public static string DestFilePath = "";
        public static string DestImagePath = "";
        public static string UniqueIDsList = "";

        public struct Tag_DBEssential
        {
            public String DBType;
            public String DBConnectionString;
            public String DBServerURL;
        }

        public struct Tag_LogEssential
        {
            public String APIURL;
            public String LogAPIPassCode;
            public String AppChannel;
            public String DestinationEmailID;
            public String FileLevel;
            public bool isClientDate;
        }

        public struct Tag_CommonIImgDetails
        {
            public String sUser;
            public String sPassword;
            public String CabinetName;

            public Int32 ProcessID;
            public String ImageServerIP;
            public String VolumeName;
            public String SiteName;
            public Int16 VolId;
            public Int32 PortId;

            public String ProcessID_DBFieldName;
            public String ImageIndex_DBField;
            //public String CabinetName_DBField;
            public String ImageServerIP_DBField;
            public String PortID_DBField;
            public String VolumeName_DBField;
            public String SiteName_DBField;
            public String VolID_DBField;
        }

        public struct Tag_CTSIntegrator
        {
            public String FilePath;
            public String ImagePath;
            public String DefFilePath;
            public String DefImagePath;
            public String UniqueIdentifier;
            public String isImageRequired;     //   Y/N
            public String Output_CSV;          //   Y/N
            public String Output_TXT;          //   Y/N
            public String Output_XML;          //   Y/N
            public String Output_JSON;         //   Y/N
            public String ImageDetailsSrc;     //   DB/CONFIG 
            public String StoredProcedure;
            
            
        }

        public enum loglevel
        {
            //Loglevel(0-error, 1-general, 2-speed, 3-debug, 4-Alert, 5-misc)

            Error,              //0
            General,            //1
            Speed,              //2
            Debug,              //3
            Alert,              //4
            Misc                //5

        }
        public struct LogAPI
        {
            //For Logwriting
            public string Username;
            public string ClientPasscode;
            public string ClientDateTime;
            public string Channel;
            public string LogMsg;
            public string ProcessName;
            public string InterfaceName;
            public string FunctionName;
            public loglevel objloglevel;
            public string CabinetName;
            //For Mail
            public bool MailTrigger;
            public string DestinationMailID;
            public string FileLevel;
        }

        public enum LogLevel
        {
            Error ,//= 1,
            General, //= 2,
            Speed, //= 3,
            Debug, //= 4,
            AlertMsg, //= 5,
            BeforeLogin //= 6
        }

        /// <name>ReadConfig</name>
        /// <summary>
        /// This function will read service config
        /// </summary>
        /// <Author>Maheep</Author>
        /// <Date>27-12-2022</Date>
        /// <returns></returns>
        public static string ReadConfig()
        {
            int TagCount = 0;
            int iLoop = 0;
            
            
            String sINIPath;
            String ConfigFilePath;
            try
            {
                sINIPath = Environment.GetEnvironmentVariable("CTS", EnvironmentVariableTarget.Machine);
                if (sINIPath == null || sINIPath == "")
                {
                    WriteLog("ReadConfig", LogLevel.Error, "Environment Variable is not set on system.", true);
                    //WriteEventLog("Error in reading NGCTSCHGConfig.Xml : Environment Variable is not set on system", "INWARD", "CTS4IWMANUALUPLOAD", "ReadNGCTSCHGConfig", GlobalClass.LogLevel.BeforeLogin, GlobalClass.DbWriteLevel.Error, "Inward", "", GlobalClass.keyWordList, GlobalClass.currentLogLevel, GlobalClass.strMaskingFlag, GlobalClass.strLogFilePath, GlobalClass.logFileSize, GlobalClass.sizeRollBackUps, true);
                    return CTS_FAIL;
                }
                //ConfigFilePath = sINIPath + "//" + Environment.GetCommandLineArgs()[1] + "//NGCTSCHGConfig.xml";
                ConfigFilePath = sINIPath + "//DBToFileConfig.xml";
                StreamReader ConfigFileReader = new StreamReader(ConfigFilePath);
                xml_ConfigDoc = new XmlDocument();
                xml_ConfigDoc.Load(ConfigFileReader);
                if (xml_ConfigDoc.GetElementsByTagName("DB_TO_FILE").Count == 1)
                {
                    xml_ConfigFileNodeList = xml_ConfigDoc.GetElementsByTagName("DB_TO_FILE");
                    xml_ConfigFileNodeList = xml_ConfigFileNodeList.Item(0).ChildNodes;
                    TagCount = xml_ConfigFileNodeList.Count;
                    iLoop = 0;

                    while (TagCount > iLoop)
                    {
                        if (xml_ConfigFileNodeList.Item(iLoop).Name == "UserName")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_CommonImgDetails.sUser = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                // Username not configured
                                WriteLog("ReadConfig", LogLevel.Error, "Username not configured.", true);
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "Password")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_CommonImgDetails.sPassword = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "Password not configured.", true);
                                // Password not configured
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "CabinetName")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_CommonImgDetails.CabinetName = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "Cabinet name not configured.", true);
                                // CabinetName not configured
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "FilePath")
                            udtTag_CTSIntegrator.FilePath = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "ImagePath")
                            udtTag_CTSIntegrator.ImagePath = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "DefaultPath_File")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_CTSIntegrator.DefFilePath = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "Default File Path not configured.", true);
                                // Default File Path must be specified.
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "DefaultPath_Img")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_CTSIntegrator.DefImagePath = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "Default Image Path not configured.", true);
                                // Default Image Path must be specified.
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "UniqueIDField")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                                udtTag_CTSIntegrator.UniqueIdentifier = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "Unique Identifier not configured.", true);
                                // Unique Identifier must be specified.
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "isImageRequired")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText == "Y")
                            {
                                udtTag_CTSIntegrator.isImageRequired = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                udtTag_CTSIntegrator.isImageRequired = "N";
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "Output_CSV")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText == "Y")
                            {
                                udtTag_CTSIntegrator.Output_CSV = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                udtTag_CTSIntegrator.Output_CSV = "N";
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "Output_TXT")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText == "Y")
                            {
                                udtTag_CTSIntegrator.Output_TXT = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                udtTag_CTSIntegrator.Output_TXT = "N";
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "Output_XML")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText == "Y")
                            {
                                udtTag_CTSIntegrator.Output_XML = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                udtTag_CTSIntegrator.Output_XML = "N";
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "Output_JSON")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText == "Y")
                            {
                                udtTag_CTSIntegrator.Output_JSON = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                udtTag_CTSIntegrator.Output_JSON = "N";
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "ImageDetails_SRC")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                                udtTag_CTSIntegrator.ImageDetailsSrc = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "Image Details Source must be specified as either CONFIG or DB.", true);
                                // Image Details Source must be specified as either CONFIG or DB.
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "ImageServerIP")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_CommonImgDetails.ImageServerIP = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                udtTag_CommonImgDetails.ImageServerIP = "";
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "PortID")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_CommonImgDetails.PortId = Convert.ToInt16(xml_ConfigFileNodeList.Item(iLoop).InnerText);

                            }
                            else
                            {
                                udtTag_CommonImgDetails.PortId = 0;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "VolumeID")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_CommonImgDetails.VolId = Convert.ToInt16(xml_ConfigFileNodeList.Item(iLoop).InnerText);

                            }
                            else
                            {
                                udtTag_CommonImgDetails.VolId = 0;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "ProcessID")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_CommonImgDetails.ProcessID = Convert.ToInt16(xml_ConfigFileNodeList.Item(iLoop).InnerText);

                            }
                            else
                            {
                                udtTag_CommonImgDetails.ProcessID = 0;
                                WriteLog("ReadConfig", LogLevel.Error, "Process ID not configured.", true);

                                //Process ID not configured 
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "VolumeName")
                            udtTag_CommonImgDetails.VolumeName = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "SiteName")
                            udtTag_CommonImgDetails.SiteName = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "ImageIndex_DBFieldName")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_CommonImgDetails.ImageIndex_DBField = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "Image Index DB field name not configured.", true);
                                // Image Index DB field name must be configured
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "ImageServerIP_DBFieldName")
                            udtTag_CommonImgDetails.ImageServerIP_DBField = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "PortID_DBFieldName")
                            udtTag_CommonImgDetails.PortID_DBField = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "VolumeID_DBFieldName")
                            udtTag_CommonImgDetails.VolID_DBField = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "VolumeName_DBFieldName")
                            udtTag_CommonImgDetails.VolumeName_DBField = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "SiteName_DBFieldName")
                            udtTag_CommonImgDetails.SiteName_DBField = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "ProcessID_DBFieldName")
                            udtTag_CommonImgDetails.ProcessID_DBFieldName = xml_ConfigFileNodeList.Item(iLoop).InnerText;

                        //else if (xml_ConfigFileNodeList.Item(iLoop).Name == "CabinetName_DBFieldName")
                        //    udtTag_CommonImgDetails.CabinetName_DBField = xml_ConfigFileNodeList.Item(iLoop).InnerText;

                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "StoredProcedureName")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                                udtTag_CTSIntegrator.StoredProcedure = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "Stored Procedure name not configured.", true);
                                // Stored Procedure name must be specified.
                                return CTS_FAIL;
                            }
                        }

                        iLoop++;
                        
                    }


                }
                else
                {
                    WriteLog("ReadConfig", LogLevel.Error, "Could not find DB_TO_FILE tag in xml configuration.", true);

                    return CTS_FAIL;
                }

                if (xml_ConfigDoc.GetElementsByTagName("DBESSENTIAL").Count == 1)
                {
                    xml_ConfigFileNodeList = null;
                    xml_ConfigFileNodeList = xml_ConfigDoc.GetElementsByTagName("DBESSENTIAL");
                    xml_ConfigFileNodeList = xml_ConfigFileNodeList.Item(0).ChildNodes;
                    iLoop = 0;
                    TagCount = 0;
                    TagCount = xml_ConfigFileNodeList.Count;
                    while (TagCount > iLoop)
                    {
                        if (xml_ConfigFileNodeList.Item(iLoop).Name == "DBType")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_DBEssential.DBType = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "DB Type must be specified.", true);
                                // DB Type must be specified
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "ConnStr")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_DBEssential.DBConnectionString = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "DB Connection String must be specified.", true);

                                // DB Connection String must be specified
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "DBURL")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_DBEssential.DBServerURL = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "DB URL must be specified.", true);

                                // DB URL must be specified
                                return CTS_FAIL;
                            }
                        }
                        iLoop++;
                    }
                }
                else
                {
                    WriteLog("ReadConfig", LogLevel.Error, "Could not find DBESSENTIAL tag in xml configuration.", true);
                    return CTS_FAIL;
                }

                if (xml_ConfigDoc.GetElementsByTagName("LOGESSENTIAL").Count == 1)
                {
                    xml_ConfigFileNodeList = null;
                    xml_ConfigFileNodeList = xml_ConfigDoc.GetElementsByTagName("LOGESSENTIAL");
                    xml_ConfigFileNodeList = xml_ConfigFileNodeList.Item(0).ChildNodes;
                    iLoop = 0;
                    TagCount = 0;
                    TagCount = xml_ConfigFileNodeList.Count;
                    while (TagCount > iLoop)
                    {
                        if (xml_ConfigFileNodeList.Item(iLoop).Name == "LogAPIURL")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_LogEssential.APIURL = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "Log API URL must be specified.", true);

                                // Log API URL must be configured
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "LogAPIPassCode")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_LogEssential.LogAPIPassCode = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "Log API Passcode must be specified.", true);
                                // Log API PassCode must be configured
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "ApplicationChannel")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_LogEssential.AppChannel = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "Log API Application Channel must be specified.", true);

                                // Log API Application channel must be configured
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "DestinationEmailID")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_LogEssential.DestinationEmailID = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "Log API Destination Email ID must be configured.", true);
                                // Log API Destination Email ID must be configured
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "LogAPIFileLevel")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "")
                            {
                                udtTag_LogEssential.FileLevel = xml_ConfigFileNodeList.Item(iLoop).InnerText;
                            }
                            else
                            {
                                WriteLog("ReadConfig", LogLevel.Error, "Log API File Level must be configured.", true);
                                // Log API Destination Email ID must be configured
                                return CTS_FAIL;
                            }
                        }
                        else if (xml_ConfigFileNodeList.Item(iLoop).Name == "isClientDate")
                        {
                            if (xml_ConfigFileNodeList.Item(iLoop).InnerText != "Y")
                            {
                                udtTag_LogEssential.isClientDate = false;
                            }
                            else
                            {
                                udtTag_LogEssential.isClientDate = true;
                            }
                        }

                        iLoop++;
                    }
                }
                else
                {
                    WriteLog("ReadConfig", LogLevel.Error, "Could not find LOGESSENTIAL tag in xml configuration.", true);
                    return CTS_FAIL;
                }

                string vStatus = ValidateConfig();
                if (vStatus != CTS_SUCCESS)
                {
                    return CTS_FAIL;
                }

                WriteLog("ReadConfig", LogLevel.Debug, "Successfully read configuration file.", false);

                return CTS_SUCCESS;
            }
            catch (Exception e)
            {
                // Exception at ReadConfig()
                WriteLog("ReadConfig", LogLevel.Error, "Exception at ReadConfig: "+ e.Message, true);

                return CTS_FAIL;
            }
        }

        /// <name>ValidateConfig</name>
        /// <summary>
        /// This function will validate service config
        /// </summary>
        /// <Author>Maheep</Author>
        /// <Date>27-12-2022</Date>
        /// <returns></returns>
        public static string ValidateConfig()
        {
            // set file/img generation path to def path if path not specified
            if (udtTag_CTSIntegrator.FilePath == "")
            {
                DestFilePath = udtTag_CTSIntegrator.DefFilePath;
            }
            else
            {
                DestFilePath = udtTag_CTSIntegrator.FilePath;
            }

            if (udtTag_CTSIntegrator.ImagePath == "")
            {
                DestImagePath = udtTag_CTSIntegrator.DefImagePath;
            }
            else
            {
                DestImagePath = udtTag_CTSIntegrator.ImagePath;
            }

            // return false if none of output file types are set to "Y"
            if (udtTag_CTSIntegrator.Output_CSV != "Y" && udtTag_CTSIntegrator.Output_XML != "Y" && udtTag_CTSIntegrator.Output_TXT != "Y" && udtTag_CTSIntegrator.Output_JSON != "Y")
            {
                // None of Output file types specified;
                WriteLog("ValidateConfig", LogLevel.Error, "None of Output file types specified.", true);

                return CTS_FAIL;
            }

            // If image src details is "CONFIG", return false even if one of params is not specified.
            if (udtTag_CTSIntegrator.ImageDetailsSrc == "CONFIG")
            {
                if (udtTag_CommonImgDetails.ImageServerIP == "" || udtTag_CommonImgDetails.PortId == 0 || udtTag_CommonImgDetails.SiteName == "" || udtTag_CommonImgDetails.CabinetName == "" || udtTag_CommonImgDetails.VolId == 0 || udtTag_CommonImgDetails.VolumeName == "")
                {
                    // One of Image details is not configured in Config
                    WriteLog("ValidateConfig", LogLevel.Error, "One of Image details is not configured in Config.", true);

                    return CTS_FAIL;
                }
            }

            // Similarly if image src details is "DB", return false even if one of DB field names is not specified.
            if (udtTag_CTSIntegrator.ImageDetailsSrc == "DB")
            {
                if (udtTag_CommonImgDetails.ImageIndex_DBField == "" || udtTag_CommonImgDetails.PortID_DBField == "" || udtTag_CommonImgDetails.SiteName_DBField == ""  || udtTag_CommonImgDetails.VolID_DBField == "" || udtTag_CommonImgDetails.VolumeName_DBField == "" || udtTag_CommonImgDetails.ProcessID_DBFieldName == "")
                {
                    // One of Image details DB field names is not configured in Config
                    WriteLog("ValidateConfig", LogLevel.Error, "One of Image details DB Field Name is not configured in Config.", true);

                    return CTS_FAIL;
                }
            }
            WriteLog("ValidateConfig", LogLevel.Debug, "Successfully validated config.", false);
            return CTS_SUCCESS;
        }

        /// <name>WriteLog</name>
        /// <summary>
        /// This function will call Log Api
        /// </summary>
        /// <Author>Maheep</Author>
        /// <Date>27-12-2022</Date>
        /// <returns></returns>
        public static void WriteLog(string Function, Enum logLevel, string logMsg, bool bFlag, bool EmailTrigger = false)
        {
            try
            {
               Task task = LogAPIAsync(Function, logLevel, logMsg, bFlag);
            }
            catch (Exception)
            {

            }

        }

        /// <name>LogAPIAsync</name>
        /// <summary>
        /// This function will write logs at server
        /// </summary>
        /// <Author>Maheep</Author>
        /// <Date>27-12-2022</Date>
        /// <returns></returns>
        public static async Task LogAPIAsync(string Function, Enum logLevel, string logMsg, bool bFlag, bool EmailTrigger = false/*, bool ClientDate = true*/)
        {
            try
            {
                string Interface = ServiceName;
                string apiUrl = udtTag_LogEssential.APIURL; 
                bool ClientDate = udtTag_LogEssential.isClientDate;

                GlobalClass.LogAPI objlogmodel = new GlobalClass.LogAPI();

                objlogmodel.ClientPasscode = udtTag_LogEssential.LogAPIPassCode;
                if (ClientDate)
                {
                    objlogmodel.ClientDateTime = Convert.ToString(DateTime.Now);  //.ToString();//("ddMMMyyyyHHmmssffff");
                }
                else
                {
                    objlogmodel.ClientDateTime = "";
                }


                objlogmodel.CabinetName = udtTag_CommonImgDetails.CabinetName;
                objlogmodel.Username = udtTag_CommonImgDetails.sUser;
                objlogmodel.ProcessName = GlobalClass.ServiceName;

                objlogmodel.Channel = udtTag_LogEssential.AppChannel;
                objlogmodel.InterfaceName = Interface;
                objlogmodel.FunctionName = Function;
                objlogmodel.objloglevel = (GlobalClass.loglevel)logLevel.GetHashCode();
                objlogmodel.LogMsg = logMsg;
                objlogmodel.MailTrigger = EmailTrigger;
                objlogmodel.DestinationMailID = udtTag_LogEssential.DestinationEmailID;  //"iffat.aara@noexternalmail.hsbc.com";//destinationmailid;
                objlogmodel.FileLevel = udtTag_LogEssential.FileLevel;

                var output = JsonConvert.SerializeObject(objlogmodel);


                string MqRetmsg = "";
                HttpClient client = new HttpClient();

                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                client.BaseAddress = new Uri(apiUrl);
                var responseTask = await client.PostAsJsonAsync("Logger", output);

                var result = responseTask; //.Result;

                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    MqRetmsg = readTask.Result;
                }


                sEvent = logMsg;
                if (!EventLog.SourceExists(sSource))
                {
                    EventLog.CreateEventSource(sSource, sLog);
                    EventLog[] Elog = EventLog.GetEventLogs();
                    foreach (EventLog e in Elog)
                        if (e.LogDisplayName == sLog)
                            e.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 5);
                }
                if (!bFlag)
                    EventLog.WriteEntry(sSource, sEvent);
                else
                    EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error);

            }
            catch (Exception)
            {
               
            }
        }


    }
}
