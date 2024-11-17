/* -----------------------------------------------------------------------
* Product/Project Name: CTS-Enhancement
* Module: CTS Integrator CTS-ITG-DBToFile
* File: DBToFileFunc.cs
* Purpose: Contains required functions for DB to file transfer



* * Functions Defined:
1) CreateCSV()
2) CreateJSON()
3) CreateXML()
4) CreateTXT()
5) GetDataFromDB()
6) GetImageForRecords()
7) StartDBtoFileTransfer()
8) QueryDatabase()
9) UpdateDataintoDB()
10) UpdateListOfUniqueIDs()

* /// <CHANGE HISTORY>
* ///
* ///-----------------------------------------------------------------------
* /// Date         :           Change By         :   Change Description
    10-03-2023              Garima Barthwal         Did proper handling for updateion of records, corrected DBInteraction params.
               


* /// </CHANGE HISTORY>
-----------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CTSLibrary;

namespace CTSIntegratorService
{
    public class DBToFileFunc
    {
        /// <name>StartDBToFileTransfer</name>
        /// <summary>
        /// This function will call main task for DB to file transfer
        /// </summary>
        /// <Author>Maheep</Author>
        /// <Date>27-12-2022</Date>
        /// <returns></returns>
        public string StartDBToFileTransfer()
        {
            Task[] tasks = null;
            tasks = new Task[0];
            try
            {
                GlobalClass.WriteLog("StartDBToFileTransfer", GlobalClass.LogLevel.Debug, "Starting DB to File transfer thread.", false);

                DBToFileFunc obj = new DBToFileFunc();
                DataTable dt = new DataTable();
                
                string sStatus = obj.GetDataFromDB(ref dt);
                if (sStatus != GlobalClass.CTS_SUCCESS)
                {
                    // Error at geeting Data from Table
                    throw new Exception("Error at GetDataFromDB");
                }
                else
                {
                    if (dt != null)
                    {
                        if (dt.DataSet.Tables["Result"] != null && dt.DataSet.Tables["Result"].Rows.Count > 0)
                        {
                            if (GlobalClass.udtTag_CTSIntegrator.Output_CSV == "Y")
                            {
                                Array.Resize(ref tasks, tasks.Length + 1);
                                tasks[tasks.Length - 1] = CreateCSV(dt.DataSet.Tables["Result"]);
                            }
                            if (GlobalClass.udtTag_CTSIntegrator.Output_XML == "Y")
                            {
                                Array.Resize(ref tasks, tasks.Length + 1);
                                tasks[tasks.Length - 1] = CreateXML(dt.DataSet.Tables["Result"]);
                            }
                            if (GlobalClass.udtTag_CTSIntegrator.Output_JSON == "Y")
                            {
                                Array.Resize(ref tasks, tasks.Length + 1);
                                tasks[tasks.Length - 1] = CreateJSON(dt.DataSet.Tables["Result"]);
                            }
                            if (GlobalClass.udtTag_CTSIntegrator.Output_TXT == "Y")
                            {
                                Array.Resize(ref tasks, tasks.Length + 1);
                                tasks[tasks.Length - 1] = CreateTXT(dt.DataSet.Tables["Result"]);
                            }

                            // Get Image for all the records.
                            if (GlobalClass.udtTag_CTSIntegrator.isImageRequired == "Y")
                            {

                                if (GlobalClass.udtTag_CTSIntegrator.Output_CSV == "Y" || GlobalClass.udtTag_CTSIntegrator.Output_XML == "Y"
                               || GlobalClass.udtTag_CTSIntegrator.Output_JSON == "Y" || GlobalClass.udtTag_CTSIntegrator.Output_TXT == "Y")
                                {
                                    Array.Resize(ref tasks, tasks.Length + 1);
                                    tasks[tasks.Length - 1] = GetImageForRecords(dt);
                                    
                                    Array.Resize(ref tasks, tasks.Length + 1);
                                    tasks[tasks.Length - 1] = UpdateListOfUniqueIDs(dt.DataSet.Tables["Result"]);

                                }
                            }
                            else
                            {
                                if (GlobalClass.udtTag_CTSIntegrator.Output_CSV == "Y" || GlobalClass.udtTag_CTSIntegrator.Output_XML == "Y"
                              || GlobalClass.udtTag_CTSIntegrator.Output_JSON == "Y" || GlobalClass.udtTag_CTSIntegrator.Output_TXT == "Y")
                                {
                                    Array.Resize(ref tasks, tasks.Length + 1);
                                    tasks[tasks.Length - 1] = UpdateListOfUniqueIDs(dt.DataSet.Tables["Result"]);

                                }
                                // Image download not required
                                GlobalClass.WriteLog("StartDBToFileTransfer", GlobalClass.LogLevel.Debug, "Image download not required.", false);

                            }

                            
                            Task.WaitAll(tasks);

                            sStatus = obj.UpdateDataIntoDB();
                        }
                        else
                        {
                            GlobalClass.WriteLog("StartDBToFileTransfer", GlobalClass.LogLevel.Debug, "No records to write to DB.", false);
                        }
                    }
                    else
                    {
                        GlobalClass.WriteLog("StartDBToFileTransfer", GlobalClass.LogLevel.Debug, "No records to write to DB.", false);

                    }
                }

                GlobalClass.WriteLog("StartDBToFileTransfer", GlobalClass.LogLevel.Debug, "Successfully completed StartDBToFileTransfer", false);

                return GlobalClass.CTS_SUCCESS;
            }
            catch (Exception e)
            {
                if (tasks != null )
                    Task.WaitAll(tasks);
                GlobalClass.WriteLog("StartDBToFileTransfer", GlobalClass.LogLevel.Error, "Exception at StartDBToFileTransfer: " + e.Message, true);

                return e.Message;

            }
        }

        /// <name>GetDataFromDB</name>
        /// <summary>
        /// This function will Get Records from Database to write into File
        /// </summary>
        /// <Author>Maheep</Author>
        /// <Date>27-12-2022</Date>
        /// <returns></returns>
        public string GetDataFromDB(ref DataTable dt)
        {
            try
            {
                Webservice.DBParameter[] iudtDBParam = null;
                
                iudtDBParam = new Webservice.DBParameter[0];
                string sSQLQuery = GlobalClass.udtTag_CTSIntegrator.StoredProcedure;
                string sQueryType = "storedproc";

                string sStatus = QueryDatabase(sSQLQuery, sQueryType, iudtDBParam, "GET", "", ref dt);
                GlobalClass.WriteLog("GetDataFromDB", GlobalClass.LogLevel.Debug, "Result at GetDataFromDB: " + sStatus, false);

                return sStatus;
            }
            catch (Exception ex)
            {
                GlobalClass.WriteLog("GetDataFromDB", GlobalClass.LogLevel.Error, "Exception at GetDataFromDB: " + ex.Message, true);

                return "Exception at GetDataFromDB(): " + ex.Message;
            }
        }

        /// <name>GetDataFromDB</name>
        /// <summary>
        /// This function will Get Records from Database to write into File
        /// </summary>
        /// <Author>Maheep</Author>
        /// <Date>27-12-2022</Date>
        /// <returns></returns>
        public string UpdateDataIntoDB()
        {
            try
            {
                // Bug ID: 1132301
                // Bug: CTS-Integrator: Unable to update records in ITG-DBToFile.
                // Added By : Garima Barthwal
                // Added on: 21-03-2023
                //Resolution : Did proper handling for updateion of records, corrected DBInteraction params.
                Webservice.DBParameter[] iudtDBParam = null;
                DataTable dt = new DataTable();

                iudtDBParam = new Webservice.DBParameter[0];
                string sSQLQuery = GlobalClass.udtTag_CTSIntegrator.StoredProcedure;
                string sQueryType = "storedproc";

                string UniqueIDsToUpdate = GlobalClass.UniqueIDsList == "" ? "" : GlobalClass.UniqueIDsList.Substring(0,GlobalClass.UniqueIDsList.LastIndexOf("#"));  // Comma seperated unique IDs to be sent as string

                string sStatus = QueryDatabase(sSQLQuery, sQueryType, iudtDBParam, "UPDATE", UniqueIDsToUpdate, ref dt);
                GlobalClass.WriteLog("UpdateDataIntoDB", GlobalClass.LogLevel.Debug, "Result at UpdateDataIntoDB: " + sStatus, false);

                if (sStatus == GlobalClass.CTS_SUCCESS)
                {
                    GlobalClass.UniqueIDsList = "";
                }
                return sStatus;
            }
            catch(Exception ex)
            {
                GlobalClass.WriteLog("UpdateDataIntoDB", GlobalClass.LogLevel.Error, "Exception at UpdateDataIntoDB: " + ex.Message, true);

                return "Exception at UpdateDataIntoDB(): " + ex.Message;
            }
        }

        /// <name>UpdateListOfUniqueIDs</name>
        /// <summary>
        /// This function will update list of records (using unique key) which have been written into file
        /// </summary>
        /// <Author>Maheep</Author>
        /// <Date>20-3-2023</Date>
        /// <returns></returns>
        public static async Task UpdateListOfUniqueIDs(DataTable dt)
        {
            await Task.Run(() =>
            {
                if (dt != null)
                {
                    if (dt.DataSet.Tables["Result"] != null && dt.DataSet.Tables["Result"].Rows.Count > 0)
                    {
                        foreach (DataRow dtRow in dt.DataSet.Tables["Result"].Rows)
                        {
                            GlobalClass.UniqueIDsList = GlobalClass.UniqueIDsList + dtRow[GlobalClass.udtTag_CTSIntegrator.UniqueIdentifier].ToString() + "#";
                        }
                    }
                }
            });
        }

        /// <name>QueryDatabase</name>
        /// <summary>
        /// This function will Query Database
        /// </summary>
        /// <Author>Maheep</Author>
        /// <Date>20-12-2022</Date>
        /// <returns></returns>
        public string QueryDatabase(string strQuery, string strQueryType, Webservice.DBParameter[] dbParams, string ActionFlag, string RecordsToUpdate, ref DataTable dtResult)
        {
            try
            {
                Webservice.DBConnectionString = GlobalClass.udtTag_DBEssential.DBConnectionString;
                Webservice.DBType = GlobalClass.udtTag_DBEssential.DBType;
                Webservice.WebserviceURL = GlobalClass.udtTag_DBEssential.DBServerURL;
                
                Array.Resize(ref dbParams, dbParams.Length + 1);
                dbParams[dbParams.Length - 1].strParamName = "ACTIONFLAG";
                dbParams[dbParams.Length - 1].strParamType = "varchar2";
                dbParams[dbParams.Length - 1].strParamDirection = "in";
                dbParams[dbParams.Length - 1].strParamValue = ActionFlag;
                dbParams[dbParams.Length - 1].intParamLength = dbParams[dbParams.Length - 1].strParamValue.Length;

                GlobalClass.WriteLog("QueryDataBase", GlobalClass.LogLevel.Debug, "ACTIONFLAG: " + ActionFlag, false);


                Array.Resize(ref dbParams, dbParams.Length + 1);
                dbParams[dbParams.Length - 1].strParamName = "objUniqueRecords";
                dbParams[dbParams.Length - 1].strParamType = "varchar2";
                dbParams[dbParams.Length - 1].strParamDirection = "in";
                dbParams[dbParams.Length - 1].strParamValue = RecordsToUpdate.Trim().ToString();
                dbParams[dbParams.Length - 1].intParamLength = dbParams[dbParams.Length - 1].strParamValue.Length;

                GlobalClass.WriteLog("QueryDataBase", GlobalClass.LogLevel.Debug, "Updated records: " + RecordsToUpdate.Trim().ToString(), false);


                if (GlobalClass.udtTag_DBEssential.DBType.Equals("ORACLE"))
                {
                    Array.Resize(ref dbParams, dbParams.Length + 1);
                    dbParams[dbParams.Length - 1].strParamName = "RecordSet";
                    dbParams[dbParams.Length - 1].strParamType = "refcursor";
                    dbParams[dbParams.Length - 1].strParamDirection = "out";
                }
                string strFetchStatus = Webservice.ExecuteDBOperation(strQuery, strQueryType, dbParams, ref dtResult, Webservice.WebserviceURL, Webservice.DBType, Webservice.DBConnectionString);
                GlobalClass.WriteLog("QueryDataBase", GlobalClass.LogLevel.Debug, "Result at QueryDatabase: " + strFetchStatus, false);

                if (strFetchStatus.Equals(GlobalClass.CTS_SUCCESS, StringComparison.CurrentCultureIgnoreCase))
                    return GlobalClass.CTS_SUCCESS;
                else
                {
                    return strFetchStatus;
                }
            }
            catch (Exception ex)
            {
                GlobalClass.WriteLog("QueryDataBase", GlobalClass.LogLevel.Error, "Exception at QueryDatabase: " + ex.Message, true);

                return "Exception at QueryDataBase():" + ex.Message;
            }
        }


        /// <name>CreateCSV</name>
        /// <summary>
        /// This function will create csv file from dt
        /// </summary>
        /// <Author>Maheep</Author>
        /// <Date>20-12-2022</Date>
        /// <returns></returns>
        public static async Task CreateCSV(DataTable dtData)
        {
            await Task.Run(() =>
            {
                StringBuilder data = new StringBuilder();
                StreamWriter objWriter;
                string DestFilePath = GlobalClass.DestFilePath + "\\CSV_" + DateTime.Now.ToString("ddMMMyyyyHHmmssffff") + ".csv";
                if (File.Exists(DestFilePath))
                {
                    File.Delete(DestFilePath);
                }

                //Taking the column names.
                for (int column = 0; column < dtData.Columns.Count; column++)
                {
                    //Making sure that end of the line, should not have comma delimiter.
                    if (dtData.Columns[column].ColumnName.ToString() != "ResultSet_Id")
                    {
                        if (column == dtData.Columns.Count - 1)
                            data.Append(dtData.Columns[column].ColumnName.ToString().Replace(",", ";"));
                        else
                            data.Append(dtData.Columns[column].ColumnName.ToString().Replace(",", ";") + ',');
                    }

                }

                data.Append(Environment.NewLine);//New line after appending columns.

                for (int row = 0; row < dtData.Rows.Count; row++)
                {
                    for (int column = 0; column < dtData.Columns.Count; column++)
                    {
                        ////Making sure that end of the line, shoould not have comma delimiter.
                        if (dtData.Columns[column].ColumnName.ToString() != "ResultSet_Id")
                        {
                            if (column == dtData.Columns.Count - 1)
                                data.Append(dtData.Rows[row][column].ToString().Replace(",", ";"));
                            else
                                data.Append(dtData.Rows[row][column].ToString().Replace(",", ";") + ',');
                        }
                    }

                    //Making sure that end of the file, should not have a new line.
                    if (row != dtData.Rows.Count - 1)
                        data.Append(Environment.NewLine);
                }

                objWriter = new StreamWriter(DestFilePath);
                objWriter.WriteLine(data);
                objWriter.Close();
                objWriter.Dispose();
            });
        }

        /// <name>CreateXML</name>
        /// <summary>
        /// This function will create xml file from dt
        /// </summary>
        /// <Author>Maheep</Author>
        /// <Date>20-12-2022</Date>
        /// <returns></returns>
        public static async Task CreateXML(DataTable dt)
        {
            await Task.Run(() =>
            {
                string DestFilePath = GlobalClass.DestFilePath + "\\XML_" + DateTime.Now.ToString("ddMMMyyyyHHmmssffff") + ".xml";
                if (File.Exists(DestFilePath))
                {
                    File.Delete(DestFilePath);
                }

                XmlTextWriter writer = new XmlTextWriter(DestFilePath, System.Text.Encoding.UTF8);
                writer.WriteStartDocument(true);
                writer.Formatting = System.Xml.Formatting.Indented;
                writer.Indentation = 2;
                writer.WriteStartElement("ResultSet");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    writer.WriteStartElement("Record" + (i + 1));
                    for (int column = 0; column < dt.Columns.Count; column++)
                    {
                        ////Making sure that end of the line, shoould not have comma delimiter.
                        if (dt.Columns[column].ColumnName.ToString() != "ResultSet_Id")
                        {
                            writer.WriteStartElement(dt.Columns[column].ColumnName.ToString());
                            writer.WriteString(dt.Rows[i][column].ToString());
                            writer.WriteEndElement();
                        }
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
                writer.Dispose();
                //dt.WriteXml(DestFilePath);
            });
        }

        /// <name>CreateTXT</name>
        /// <summary>
        /// This function will create text file from dt
        /// </summary>
        /// <Author>Maheep</Author>
        /// <Date>20-3-2023</Date>
        /// <returns></returns>
        public static async Task CreateTXT(DataTable dt)
        {
            await Task.Run(() =>
            {
                StreamWriter objWriter;
                var TXTString = new StringBuilder();
                string DestFilePath = GlobalClass.DestFilePath + "\\TXT_" + DateTime.Now.ToString("ddMMMyyyyHHmmssffff") + ".txt";
                if (File.Exists(DestFilePath))
                {
                    File.Delete(DestFilePath);
                }

                if (dt.Rows.Count > 0)
                {
                    for (int column = 0; column < dt.Columns.Count; column++)
                    {
                        //Making sure that end of the line, shoould not have comma delimiter.
                        if (dt.Columns[column].ColumnName.ToString() != "ResultSet_Id")
                        {
                            TXTString.Append(dt.Columns[column].ColumnName.ToString().PadRight(20, ' '));
                        }

                    }

                    TXTString.Append(Environment.NewLine);

                    for (int row = 0; row < dt.Rows.Count; row++)
                    {
                        for (int column = 0; column < dt.Columns.Count; column++)
                        {
                            ////Making sure that end of the line, shoould not have comma delimiter.
                            if (dt.Columns[column].ColumnName.ToString() != "ResultSet_Id")
                            {
                                string tempStr = dt.Rows[row][column] == null ? " " : dt.Rows[row][column].ToString();
                                TXTString.Append(tempStr.ToString().ToUpper().PadRight(20, ' '));
                            }
                        }

                        //Making sure that end of the file, should not have a new line.
                        if (row != dt.Rows.Count - 1)
                            TXTString.Append(Environment.NewLine);
                    }
                }

                objWriter = new StreamWriter(DestFilePath);
                objWriter.WriteLine(TXTString);
                objWriter.Close();
                objWriter.Dispose();
            });
        }

        /// <name>CreateJSON</name>
        /// <summary>
        /// This function will create json file from dt
        /// </summary>
        /// <Author>Maheep</Author>
        /// <Date>20-3-2023</Date>
        /// <returns></returns>
        public static async Task CreateJSON(DataTable dt)
        {
            await Task.Run(() =>
            {
                StreamWriter objWriter;
                var JSONString = new StringBuilder();
                string DestFilePath = GlobalClass.DestFilePath + "\\JSON_" + DateTime.Now.ToString("ddMMMyyyyHHmmssffff") + ".json";
                if (File.Exists(DestFilePath))
                {
                    File.Delete(DestFilePath);
                }

                if (dt.Rows.Count > 0)
                {
                    JSONString.Append("[" + Environment.NewLine);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        JSONString.Append("{");
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            if (dt.Columns[j].ColumnName.ToString() != "ResultSet_Id")
                            {
                                if (j < dt.Columns.Count - 1)
                                {
                                    JSONString.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":" + "\"" + dt.Rows[i][j].ToString() + "\",");
                                }
                                else if (j == dt.Columns.Count - 1)
                                {
                                    JSONString.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":" + "\"" + dt.Rows[i][j].ToString() + "\"");
                                }
                            }
                        }
                        if (i == dt.Rows.Count - 1)
                        {
                            JSONString.Append("}" + Environment.NewLine);
                        }
                        else
                        {
                            JSONString.Append("}," + Environment.NewLine);
                        }
                    }
                    JSONString.Append("]");
                }

                objWriter = new StreamWriter(DestFilePath);
                objWriter.WriteLine(JSONString);
                objWriter.Close();
                objWriter.Dispose();
            });
        }

        /// <name>GetImageForRecords</name>
        /// <summary>
        /// This function will fetch images for records
        /// </summary>
        /// <Author>Maheep</Author>
        /// <Date>20-3-2023</Date>
        /// <returns></returns>
        public static async Task GetImageForRecords(DataTable dt)
        {
            await Task.Run(() =>
            {
                string sSiteName;
                OmniFlow.CTSError udtError = new OmniFlow.CTSError();
                CTSWrapper.clsGlobalDecl.WMTImageServerConnectInfo udtWMTImageServerConnectInfo = new CTSWrapper.clsGlobalDecl.WMTImageServerConnectInfo();
                
                if (dt != null)
                {
                    if (dt.DataSet.Tables["Result"] != null && dt.DataSet.Tables["Result"].Rows.Count > 0)
                    {
                        DirectoryInfo di = new DirectoryInfo(GlobalClass.DestImagePath);
                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        if (GlobalClass.udtTag_CTSIntegrator.ImageDetailsSrc == "CONFIG")
                        {
                            GlobalClass.objOmniFlow.ImageServerIP = GlobalClass.udtTag_CommonImgDetails.ImageServerIP;
                            GlobalClass.objOmniFlow.VolumeName = GlobalClass.udtTag_CommonImgDetails.VolumeName;
                            GlobalClass.objOmniFlow.PortId = Convert.ToInt32(GlobalClass.udtTag_CommonImgDetails.PortId);
                            GlobalClass.objOmniFlow.VolId = Convert.ToInt16(GlobalClass.udtTag_CommonImgDetails.VolId);
                            GlobalClass.objOmniFlow.ProcessId = Convert.ToInt32(GlobalClass.udtTag_CommonImgDetails.ProcessID);
                            sSiteName = GlobalClass.udtTag_CommonImgDetails.SiteName;
                            udtWMTImageServerConnectInfo = GlobalClass.objOmniFlow.GetWMTImageServerConnectInfo;

                            foreach (DataRow dtRow in dt.DataSet.Tables["Result"].Rows)
                            {
                                udtWMTImageServerConnectInfo.Image_Path = GlobalClass.DestImagePath + "\\" + dtRow[GlobalClass.udtTag_CTSIntegrator.UniqueIdentifier].ToString() + ".tif";
                                
                                if (!File.Exists(udtWMTImageServerConnectInfo.Image_Path))
                                {
                                    //File.Delete(udtWMTImageServerConnectInfo.Image_Path);
                                    //}

                                    udtError = GlobalClass.objOmniDocs.GetImageFromOD(ref udtWMTImageServerConnectInfo, Convert.ToInt32(dtRow[GlobalClass.udtTag_CommonImgDetails.ImageIndex_DBField]), 1, 1, 1, 1, sSiteName);
                                    if (udtError.main_code != GlobalClass.CTS_SUCCESS)
                                    {
                                        GlobalClass.WriteLog("GetImageForRecords", GlobalClass.LogLevel.Error, "Error While fecthing Image from OD: " + udtError.main_code + " for " + dtRow[GlobalClass.udtTag_CTSIntegrator.UniqueIdentifier].ToString(), true);

                                        //Write Log "Error While fecthing Image from OD: " + udtError.main_code = " for " + <Unique ID>;
                                    }
                                }
                                //else
                                //{
                                //    //Append Unique ID to list/string with comma separator;
                                //    GlobalClass.UniqueIDsList = GlobalClass.UniqueIDsList + dtRow[GlobalClass.udtTag_CTSIntegrator.UniqueIdentifier].ToString() + "#";
                                //}
                            }
                        }
                        else if (GlobalClass.udtTag_CTSIntegrator.ImageDetailsSrc == "DB")
                        {
                            foreach (DataRow dtRow in dt.DataSet.Tables["Result"].Rows)
                            {
                                GlobalClass.objOmniFlow.ImageServerIP = dtRow[GlobalClass.udtTag_CommonImgDetails.ImageServerIP_DBField].ToString();
                                GlobalClass.objOmniFlow.VolumeName = dtRow[GlobalClass.udtTag_CommonImgDetails.VolumeName_DBField].ToString();
                                GlobalClass.objOmniFlow.PortId = Convert.ToInt32(dtRow[GlobalClass.udtTag_CommonImgDetails.PortID_DBField].ToString());
                                GlobalClass.objOmniFlow.VolId = Convert.ToInt16(dtRow[GlobalClass.udtTag_CommonImgDetails.VolID_DBField].ToString());
                                GlobalClass.objOmniFlow.ProcessId = Convert.ToInt32(dtRow[GlobalClass.udtTag_CommonImgDetails.ProcessID_DBFieldName].ToString());
                                sSiteName = dtRow[GlobalClass.udtTag_CommonImgDetails.SiteName_DBField].ToString();
                                udtWMTImageServerConnectInfo = GlobalClass.objOmniFlow.GetWMTImageServerConnectInfo;


                                udtWMTImageServerConnectInfo.Image_Path = GlobalClass.DestImagePath + "\\" + dtRow[GlobalClass.udtTag_CTSIntegrator.UniqueIdentifier].ToString() + ".tif";
                                if (!File.Exists(udtWMTImageServerConnectInfo.Image_Path))
                                {
                                    //    File.Delete(udtWMTImageServerConnectInfo.Image_Path);
                                    //}

                                    udtError = GlobalClass.objOmniDocs.GetImageFromOD(ref udtWMTImageServerConnectInfo, Convert.ToInt32(dtRow[GlobalClass.udtTag_CommonImgDetails.ImageIndex_DBField]), 1, 1, 1, 1, sSiteName);
                                    if (udtError.main_code != GlobalClass.CTS_SUCCESS)
                                    {
                                        GlobalClass.WriteLog("GetImageForRecords", GlobalClass.LogLevel.Error, "Error While fecthing Image from OD: " + udtError.main_code + " for " + dtRow[GlobalClass.udtTag_CTSIntegrator.UniqueIdentifier].ToString(), true);

                                        //Write Log "Error While fecthing Image from OD: " + udtError.main_code = " for " + <Unique ID>;
                                    }
                                }
                                //else
                                //{
                                //    //Append Unique ID to list/string with comma separator;
                                //    GlobalClass.UniqueIDsList = GlobalClass.UniqueIDsList + dtRow[GlobalClass.udtTag_CTSIntegrator.UniqueIdentifier].ToString() + "#";
                                //}
                            }

                        }
                    }
                }


            });
        }
        
    }
}
