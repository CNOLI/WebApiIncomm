using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using System.IO;
using System.Net;
using System.Data.Common;
using Hub_ConciliacionDistribuidores.Models;

namespace Hub_ConciliacionDistribuidores
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                // Variables globales
                string conexion = config.GetSection("SQL").Value;

                string rutaArchivoInput = config.GetSection("HubTISIInfo:rutaArchivoInput").Value;
                string rutaArchivoOutputConciliados = config.GetSection("HubTISIInfo:rutaArchivoOutputConciliados").Value;
                string rutaArchivoOutputNoConciliados = config.GetSection("HubTISIInfo:rutaArchivoOutputNoConciliados").Value;
                string rutaBackup = config.GetSection("HubTISIInfo:rutaBackup").Value;

                string diaProceso = config.GetSection("HubTISIInfo:diaProceso").Value;

                string emailServidor = config.GetSection("HubTISIInfo:emailServidor").Value;
                string emailDestinatario = config.GetSection("HubTISIInfo:emailDestinatario").Value;
                string emailContrasena = config.GetSection("HubTISIInfo:emailContrasena").Value;
                string emailPuerto = config.GetSection("HubTISIInfo:emailPuerto").Value;
                string emailSSL = config.GetSection("HubTISIInfo:emailSSL").Value;

                string emailCorreoSoporte = config.GetSection("HubTISIInfo:emailCorreoSoporte").Value;

                string nombreArchivoInput;
                string nombreArchivoConciliados;
                string nombreArchivoNoConciliados;
                string vc_etapa = "";

                //0) Obtener configuración del distribuidor.
                List<Distribuidor> ls_dist = new List<Distribuidor>();

                Console.WriteLine("1) Obtener configuración de los distribuidores");
                vc_etapa = "Obtener configuración de los distribuidores";

                ls_dist = sel_distribuidor_conciliacion_config(conexion);

                RespuestaProceso respuesta = new RespuestaProceso();

                foreach (var dist in ls_dist)
                {
                    // Renombrar rutas.
                    vc_etapa = dist.vc_desc_distribuidor + " - " + "Renombrar rutas.";
                    DateTime fecha;
                    try
                    {
                        if (diaProceso.Contains("/"))
                        {
                            fecha = Convert.ToDateTime(diaProceso);
                        }
                        else
                        {
                            fecha = DateTime.Now.AddDays(Convert.ToDouble(diaProceso));
                        }
                    }
                    catch (Exception ex)
                    {
                        fecha = DateTime.Now.AddDays(-1);
                    }

                    rutaBackup = RenombrarRutaArchivo(rutaBackup, dist, fecha);
                    rutaArchivoInput = RenombrarRutaArchivo(rutaArchivoInput, dist, fecha);
                    rutaArchivoOutputConciliados = RenombrarRutaArchivo(rutaArchivoOutputConciliados, dist, fecha);
                    rutaArchivoOutputNoConciliados = RenombrarRutaArchivo(rutaArchivoOutputNoConciliados, dist, fecha);
                    dist.vc_ruta_archivo = RenombrarRutaArchivo(dist.vc_ruta_archivo, dist, fecha);

                    //Obtener nombre archivo
                    vc_etapa = dist.vc_desc_distribuidor + " - " + "Obtener nombre de archivo.";
                    nombreArchivoInput = rutaArchivoInput.Split(@"\"[0])[rutaArchivoInput.Split(@"\"[0]).Length - 1];
                    nombreArchivoConciliados = rutaArchivoOutputConciliados.Split(@"\"[0])[rutaArchivoOutputConciliados.Split(@"\"[0]).Length - 1];
                    nombreArchivoNoConciliados = rutaArchivoOutputNoConciliados.Split(@"\"[0])[rutaArchivoOutputNoConciliados.Split(@"\"[0]).Length - 1];

                    //Verificar Carpetas de salida

                    vc_etapa = dist.vc_desc_distribuidor + " - " + "Verificar carpetas de salida.";
                    if (!Directory.Exists(rutaBackup))
                        Directory.CreateDirectory(rutaBackup);

                    if (!Directory.Exists(rutaArchivoOutputConciliados.Replace(@"\" + nombreArchivoConciliados, "")))
                        Directory.CreateDirectory(rutaArchivoOutputConciliados.Replace(@"\" + nombreArchivoConciliados, ""));

                    if (!Directory.Exists(rutaArchivoOutputNoConciliados.Replace(@"\" + nombreArchivoNoConciliados, "")))
                        Directory.CreateDirectory(rutaArchivoOutputNoConciliados.Replace(@"\" + nombreArchivoNoConciliados, ""));


                    //1) Obtener archivo conciliacion desde el Distribuidor SFTP
                    if (dist.bi_obtener_archivo)
                    {
                        //FTP
                        if (dist.nu_id_metodo == 1)
                        {
                            respuesta = Descargar_Archivo_FTP(dist.vc_ip_archivo, dist.nu_puerto_archivo, dist.vc_usuario_archivo, dist.vc_contrasena_archivo, dist.vc_ruta_archivo, rutaArchivoInput);

                            if (!respuesta.bi_estado)
                            {
                                throw new InvalidOperationException(vc_etapa + " - " + respuesta.vc_mensaje);
                            }

                        }

                        // SFTP
                        if (dist.nu_id_metodo == 2)
                        {
                            respuesta = Descargar_Archivo_SFTP(dist.vc_ip_archivo, dist.nu_puerto_archivo, dist.vc_usuario_archivo, dist.vc_contrasena_archivo, dist.vc_ruta_archivo, rutaArchivoInput);

                            if (!respuesta.bi_estado)
                            {
                                throw new InvalidOperationException(vc_etapa + " - " + respuesta.vc_mensaje);
                            }
                        }
                    }


                    //2) Leer el archivo de conciliación y realizar el proceso de conciliación.
                    vc_etapa = dist.vc_desc_distribuidor + " - " + "Leer archivo de conciliación.";
                    DataTable datos = LeerArchivoTXT(rutaArchivoInput, "|");

                    vc_etapa = dist.vc_desc_distribuidor + " - " + "Guardar datos del archivo de conciliación.";
                    SqlCommand cmd_cab = null;
                    foreach (DataRow item in datos.Rows)
                    {
                        if (item[0].ToString() == "C")
                        {
                            Distribuidor_Conciliacion dc = new Distribuidor_Conciliacion();
                            dc.vc_cod_distribuidor = item[1].ToString();
                            dc.nu_total_trx = item[2].ToDecimal();
                            dc.nu_imp_trx = item[3].ToDecimal();

                            dc.vc_nom_archivo = nombreArchivoInput;
                            dc.vc_archivo = File.ReadAllText(rutaArchivoInput);
                            dc.dt_fecha = nombreArchivoInput.Substring(0,8);

                            cmd_cab = ins_distribuidor_conciliacion(conexion, dc);

                        }
                        if (item[0].ToString() == "D")
                        {
                            Distribuidor_Conciliacion_Det dcd = new Distribuidor_Conciliacion_Det();
                            dcd.nu_id_conciliacion = cmd_cab.Parameters["@nu_tran_pkey"].Value.ToDecimal();
                            dcd.vc_cod_comercio = item[1].ToString().Replace("\r", "");
                            dcd.nu_id_trx = item[2].ToDecimal();
                            dcd.vc_id_ref_trx_distribuidor = item[3].ToString().Replace("\r", "");
                            dcd.dt_fecha = item[4].ToString().Replace("\r", "");
                            dcd.ti_hora = item[5].ToString().Substring(0, 2) + ":" + item[5].ToString().Substring(2, 2) + ":" + item[5].ToString().Substring(4, 2) + "." + item[5].ToString().Substring(6, 2);
                            dcd.nu_id_producto = item[6].ToDecimal();
                            dcd.nu_precio = item[7].ToDecimal();
                            dcd.vc_numero_servicio = item[8].ToString().Replace("\r", "");
                            dcd.vc_nro_doc_pago = item[9].ToString().Replace("\r", "");

                            ins_distribuidor_conciliacion_det(conexion, dcd);

                        }
                    }

                    //Proceso de conciliacion


                    //Mover y generar archivos


                    //3) Informar los archivos de resultado por Email.
                    if (dist.bi_resultado_email)
                    {

                    }

                }

            }
            catch (Exception ex)
            {
                //Enviar Correo
                throw;
            }
        }
        private static string RenombrarRutaArchivo(string rutaArchivo, Distribuidor distribuidor, DateTime fecha)
        {
            rutaArchivo = rutaArchivo.Replace("[YYYY]", fecha.ToString("yyyy"));
            rutaArchivo = rutaArchivo.Replace("[MM]", fecha.ToString("MM"));
            rutaArchivo = rutaArchivo.Replace("[DD]", fecha.ToString("dd"));

            rutaArchivo = rutaArchivo.Replace("[YYYYMMDD]", fecha.ToString("yyyyMMdd"));
            rutaArchivo = rutaArchivo.Replace("[HHMMSS]", fecha.ToString("HHmmss"));

            rutaArchivo = rutaArchivo.Replace("[NU_ID_DISTRIBUIDOR]", distribuidor.nu_id_distribuidor.ToString());
            rutaArchivo = rutaArchivo.Replace("[VC_COD_DISTRIBUIDOR]", distribuidor.vc_cod_distribuidor.ToString());

            return rutaArchivo;
        }
        public static RespuestaProceso Descargar_Archivo_FTP(string servidor, int? puerto, string usuario, string contrasena, string rutaDistribuidor, string rutaHub)
        {
            RespuestaProceso rp = new RespuestaProceso();
            try
            {
                string RutaCompletaFTP = ("ftp:\\\\" + servidor + "\\" + rutaDistribuidor).Replace("\\", "/");
                var request = (FtpWebRequest)WebRequest.Create(RutaCompletaFTP);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                try
                {
                    FileInfo toDownload = new FileInfo(rutaHub);

                    if (toDownload.Exists)
                    {
                        File.Delete(rutaHub);
                    }
                }
                catch (Exception ex) { }

                request.Credentials = new NetworkCredential(usuario, contrasena);
                using (var response = (FtpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var memoryStream = new MemoryStream())
                {
                    responseStream?.CopyTo(memoryStream);
                    string archivo_base64 = Convert.ToBase64String(memoryStream.ToArray(), 0, memoryStream.ToArray().Length);
                    File.WriteAllBytes(rutaHub, Convert.FromBase64String(archivo_base64));
                }

                rp.bi_estado = true;
                rp.vc_mensaje = "El proceso de carga del archivo al FTP fue realizado correctamente.";

            }
            catch (WebException ex)
            {
                rp.bi_estado = false;
                rp.vc_mensaje = ex.Message;

            }
            catch (Exception ex)
            {
                rp.bi_estado = false;
                rp.vc_mensaje = ex.Message;
            }
            return rp;
        }
        public static RespuestaProceso Cargar_Archivo_FTP(string servidor, int? puerto, string usuario, string contrasena, string rutaHub, string rutaDistribuidor)
        {
            RespuestaProceso rp = new RespuestaProceso();
            try
            {
                string RutaCompletaFTP = ("ftp:\\\\" + servidor + "\\" + rutaDistribuidor).Replace("\\", "/");
                FtpWebRequest ftpReq = (FtpWebRequest)WebRequest.Create(RutaCompletaFTP);
                ftpReq.UseBinary = true;
                ftpReq.Method = WebRequestMethods.Ftp.UploadFile;
                ftpReq.Credentials = new NetworkCredential(usuario, contrasena);

                byte[] b = File.ReadAllBytes(rutaHub);

                ftpReq.ContentLength = b.Length;
                using (Stream s = ftpReq.GetRequestStream())
                {
                    s.Write(b, 0, b.Length);
                }

                FtpWebResponse ftpResp = (FtpWebResponse)ftpReq.GetResponse();

                if (ftpResp != null)
                {
                    rp.bi_estado = true;
                    rp.vc_mensaje = "El proceso de carga del archivo al FTP fue realizado correctamente.";
                }
                else
                {
                    rp.bi_estado = false;
                    rp.vc_mensaje = "No se realizó la carga al FTP correctamente.";
                }
            }
            catch (WebException ex)
            {
                rp.bi_estado = false;
                rp.vc_mensaje = ex.Message;

            }
            catch (Exception ex)
            {
                rp.bi_estado = false;
                rp.vc_mensaje = ex.Message;
            }
            return rp;
        }
        private static RespuestaProceso Descargar_Archivo_SFTP(string servidor, int? puerto, string usuario, string contrasena, string rutaDistribuidor, string rutaHub)
        {
            RespuestaProceso rp = new RespuestaProceso();
            try
            {
                using (SftpClient sftp = new SftpClient(servidor, puerto ?? 22, usuario, contrasena))
                {
                    sftp.Connect();

                    try
                    {
                        FileInfo toDownload = new FileInfo(rutaHub);

                        if (toDownload.Exists)
                        {
                            File.Delete(rutaHub);
                        }
                    }
                    catch (Exception ex) { }
                                       
                    using (Stream fileStream = File.OpenWrite(rutaHub))
                    {
                        sftp.DownloadFile(rutaDistribuidor, fileStream);
                    }

                    sftp.Disconnect();

                    rp.bi_estado = true;
                    rp.vc_mensaje = "El proceso de descarga del archivo desde el SFTP fue realizado correctamente.";
                }
            }
            catch (Renci.SshNet.Common.SshConnectionException)
            {
                rp.bi_estado = false;
                rp.vc_mensaje = "No se logró conectar con el servidor SFTP del distribuidor.";
            }
            catch (System.Net.Sockets.SocketException)
            {
                rp.bi_estado = false;
                rp.vc_mensaje = "No se logró establecer el socket con el servidor SFTP del distribuidor.";
            }
            catch (Renci.SshNet.Common.SshAuthenticationException)
            {
                rp.bi_estado = false;
                rp.vc_mensaje = "No se logró autenticar el servidor SFTP del distribuidor.";
            }
            catch (Exception ex)
            {
                rp.bi_estado = false;
                switch (ex.Message)
                {
                    case "No such file":
                        rp.vc_mensaje = "No se encontró el archivo en la ruta SFTP del distribuidor";
                        break;
                    default:
                        rp.vc_mensaje = ex.Message;
                        break;
                }
            }
            return rp;
        }
        private static RespuestaProceso Cargar_Archivo_SFTP(string servidor, int? puerto, string usuario, string contrasena, string rutaHub, string rutaDistribuidor)
        {
            RespuestaProceso rp = new RespuestaProceso();
            try
            {
                using (SftpClient sftp = new SftpClient(servidor, puerto ?? 21, usuario, contrasena))
                {

                    sftp.Connect();

                    try { sftp.DeleteFile(rutaDistribuidor); } catch (Exception ex) { }

                    using (Stream fileStream = File.OpenRead(rutaHub))
                    {
                        sftp.UploadFile(fileStream, rutaDistribuidor);
                    }

                    sftp.Disconnect();

                    rp.bi_estado = true;
                    rp.vc_mensaje = "El proceso de carga del archivo al SFTP fue realizado correctamente.";
                }
            }
            catch (Renci.SshNet.Common.SshConnectionException)
            {
                rp.bi_estado = false;
                rp.vc_mensaje = "No se logró conectar con el servidor SFTP.";
            }
            catch (System.Net.Sockets.SocketException)
            {
                rp.bi_estado = false;
                rp.vc_mensaje = "No se logró establecer el socket con el servidor SFTP.";
            }
            catch (Renci.SshNet.Common.SshAuthenticationException)
            {
                rp.bi_estado = false;
                rp.vc_mensaje = "No se logró autenticar el servidor SFTP.";
            }
            catch (Exception ex)
            {
                rp.bi_estado = false;
                switch (ex.Message)
                {
                    case "no such file":
                        rp.vc_mensaje = "No se encontró el archivo en la ruta SFTP.";
                        break;
                    default:
                        rp.vc_mensaje = ex.Message;
                        break;
                }
            }
            return rp;
        }

        private static DataTable LeerArchivoTXT(string rutaArchivo, string delimitador)
        {
            DataTable dt_datos = new DataTable();
            try
            {
                FileInfo toDownload = new FileInfo(rutaArchivo);

                if (toDownload.Exists)
                {
                    string ContenidoArchivo = File.ReadAllText(rutaArchivo);

                    string[] rows = ContenidoArchivo.Split('\n');
                    int cant_columnas_total = 0;
                    foreach (string s in rows)
                    {
                        int cant_columnas = 0;
                        string[] columns = s.Split(delimitador[0]);
                        cant_columnas = columns.Length;
                        if (cant_columnas > cant_columnas_total)
                        {
                            cant_columnas_total = cant_columnas;
                        }
                    }
                    for (int i = 0; i < cant_columnas_total; i++)
                    {
                        dt_datos.Columns.Add("" + i.ToString() + "", typeof(String));
                    }

                    foreach (string s in rows)
                    {
                        string[] columns = s.Split(delimitador[0]);
                        int columna = 0;
                        var dataRow = dt_datos.NewRow();
                        foreach (string item in columns)
                        {
                            dataRow["" + columna.ToString() + ""] = columns[columna];
                            columna++;
                        }

                        dt_datos.Rows.Add(dataRow);
                    }
                }
                else
                {
                    //Archivo no existe
                }
            }
            catch (Exception ex)
            {
                //Archivo con errores
            }
            return dt_datos;

        }
        public static List<Distribuidor> sel_distribuidor_conciliacion_config(string conexion)
        {
            List<Distribuidor> ls = new List<Distribuidor>();
            Distribuidor model = new Distribuidor();

            using (var cn = new SqlConnection(conexion))
            {
                cn.Open();
                using (var cmd = new SqlCommand("TISI_GLOBAL.USP_SEL_DISTRIBUIDOR_CONCILIACION_CONFIG", cn))
                {
                    model.nu_tran_ruta = 1;
                    cmd.CommandType = CommandType.StoredProcedure;
                    Models.UtilSql.iGet(cmd, model);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        model = new Distribuidor();

                        if (Convertidor_oR.Ec(dr, "nu_id_distribuidor"))
                            model.nu_id_distribuidor = dr["nu_id_distribuidor"].ToDecimal();
                        if (Convertidor_oR.Ec(dr, "vc_cod_distribuidor"))
                            model.vc_cod_distribuidor = dr["vc_cod_distribuidor"].ToString();
                        if (Convertidor_oR.Ec(dr, "vc_desc_distribuidor"))
                            model.vc_desc_distribuidor = dr["vc_desc_distribuidor"].ToString();
                        if (Convertidor_oR.Ec(dr, "bi_obtener_archivo"))
                            model.bi_obtener_archivo = dr["bi_obtener_archivo"].ToBool();
                        if (Convertidor_oR.Ec(dr, "nu_id_metodo"))
                            model.nu_id_metodo = dr["nu_id_metodo"].ToInt();
                        if (Convertidor_oR.Ec(dr, "vc_ip_archivo"))
                            model.vc_ip_archivo = dr["vc_ip_archivo"].ToString();
                        if (Convertidor_oR.Ec(dr, "nu_puerto_archivo"))
                            model.nu_puerto_archivo = dr["nu_puerto_archivo"].ToInt();
                        if (Convertidor_oR.Ec(dr, "vc_usuario_archivo"))
                            model.vc_usuario_archivo = dr["vc_usuario_archivo"].ToString();
                        if (Convertidor_oR.Ec(dr, "vc_contrasena_archivo"))
                            model.vc_contrasena_archivo = dr["vc_contrasena_archivo"].ToString();
                        if (Convertidor_oR.Ec(dr, "vc_ruta_archivo"))
                            model.vc_ruta_archivo = dr["vc_ruta_archivo"].ToString();
                        if (Convertidor_oR.Ec(dr, "bi_resultado_email"))
                            model.bi_resultado_email = dr["bi_resultado_email"].ToBool();
                        if (Convertidor_oR.Ec(dr, "vc_email"))
                            model.vc_email = dr["vc_email"].ToString();

                        ls.Add(model);
                    }

                    return ls;
                }
            }
        }
        public static SqlCommand ins_distribuidor_conciliacion(string conexion, Distribuidor_Conciliacion model)
        {
            SqlTransaction tran = null;
            using (var cn = new SqlConnection(conexion))
            {
                cn.Open();
                tran = cn.BeginTransaction();
                using (var cmd = new SqlCommand("TISI_GLOBAL.USP_INS_DISTRIBUIDOR_CONCILIACION", cn, tran))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@VC_COD_DISTRIBUIDOR", model.vc_cod_distribuidor);
                    cmd.Parameters.AddWithValue("@VC_NOM_ARCHIVO", model.vc_nom_archivo);
                    cmd.Parameters.AddWithValue("@VC_ARCHIVO", model.vc_archivo);
                    cmd.Parameters.AddWithValue("@DT_FECHA", model.dt_fecha);
                    cmd.Parameters.AddWithValue("@NU_TOTAL_TRX", model.nu_total_trx);
                    cmd.Parameters.AddWithValue("@NU_IMP_TRX", model.nu_imp_trx);

                    Models.UtilSql.iIns(cmd, model);
                    cmd.ExecuteNonQuery();

                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                    {
                        tran.Rollback();
                        return cmd;
                    }
                    tran.Commit();
                    return cmd;
                }
            }


        }
        public static SqlCommand ins_distribuidor_conciliacion_det(string conexion, Distribuidor_Conciliacion_Det model)
        {
            SqlTransaction tran = null;
            using (var cn = new SqlConnection(conexion))
            {
                cn.Open();
                tran = cn.BeginTransaction();
                using (var cmd = new SqlCommand("TISI_GLOBAL.USP_INS_DISTRIBUIDOR_CONCILIACION_DET", cn, tran))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@NU_ID_CONCILIACION", model.nu_id_conciliacion);
                    cmd.Parameters.AddWithValue("@VC_COD_COMERCIO", model.vc_cod_comercio);
                    cmd.Parameters.AddWithValue("@NU_ID_TRX", model.nu_id_trx);
                    cmd.Parameters.AddWithValue("@VC_ID_REF_TRX_DISTRIBUIDOR", model.vc_id_ref_trx_distribuidor);
                    cmd.Parameters.AddWithValue("@DT_FECHA", model.dt_fecha);
                    cmd.Parameters.AddWithValue("@TI_HORA", model.ti_hora);
                    cmd.Parameters.AddWithValue("@NU_ID_PRODUCTO", model.nu_id_producto);
                    cmd.Parameters.AddWithValue("@NU_PRECIO", model.nu_precio);
                    cmd.Parameters.AddWithValue("@VC_NUMERO_SERVICIO", model.vc_numero_servicio);
                    cmd.Parameters.AddWithValue("@VC_NRO_DOC_PAGO", model.vc_nro_doc_pago);

                    Models.UtilSql.iIns(cmd, model);
                    cmd.ExecuteNonQuery();

                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                    {
                        tran.Rollback();
                        return cmd;
                    }
                    tran.Commit();
                    return cmd;
                }

            }
        }

    }
}
