using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using MimeKit;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace wa_api_incomm.Smtp
{
    public class Send
    {
        private readonly Serilog.ILogger _logger;
        public Send(Serilog.ILogger logger)
        {
            _logger = logger;
        }
        //public void Email(string id_trx, string email, string titulo, string body, string email_envio, string contraseña_email, string smtp_email, int puerto, bool ssl,string empresa, SqlConnection cn)
        //{
        //    try
        //    {

        //        MailMessage correo = new MailMessage();
        //        correo.To.Clear();

        //        correo.Body = body;
        //        correo.Subject = titulo;
        //        correo.IsBodyHtml = true;
        //        correo.BodyEncoding = Encoding.UTF8;


        //        correo.From = new MailAddress(email_envio, empresa);
        //        correo.To.Add(email);

        //        SmtpClient envio = new SmtpClient(smtp_email);
        //        envio.Port = puerto;
        //        envio.DeliveryMethod = SmtpDeliveryMethod.Network;
        //        envio.UseDefaultCredentials = false;
        //        envio.Credentials = new NetworkCredential(email_envio, contraseña_email);
        //        envio.EnableSsl = ssl;


        //        string userState = "test message1";
        //        envio.SendAsync(correo, userState);

        //    }
        //    catch (Exception ex) {                
        //        _logger.Error(ex, "id_trx: " + id_trx + ex.Message);

        //        ReenvioMensajeModel rm = new ReenvioMensajeModel();
        //        rm.nu_id_trx = id_trx;
        //        rm.ch_tipo_mensaje = "M";
        //        var cmd = insSendInfoError(cn, rm);
        //    }

        //}
        public void Email(string id_trx, string email, string titulo, string body, string email_envio, string contraseña_email, string smtp_email, int puerto, bool ssl, string empresa, SqlConnection cn)
        {
            try
            {

                MimeMessage correo = new MimeMessage();
                correo.To.Clear();

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = body;
                correo.Body = bodyBuilder.ToMessageBody();
                correo.Subject = titulo;


                correo.From.Add(new MailboxAddress(empresa, email_envio));

                correo.To.Add(new MailboxAddress("", email.Trim()));

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                    client.Connect(smtp_email, puerto, false);

                    client.Authenticate(email_envio, contraseña_email);

                    client.Send(correo);

                    client.Disconnect(true);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "id_trx: " + id_trx + ex.Message);

                ReenvioMensajeModel rm = new ReenvioMensajeModel();
                rm.nu_id_trx = id_trx;
                rm.ch_tipo_mensaje = "M";
                var cmd = insSendInfoError(cn, rm);
            }

        }

        private static SqlCommand insSendInfoError(SqlConnection cn, ReenvioMensajeModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_reenvio_mensaje", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_trx", model.nu_id_trx);
                cmd.Parameters.AddWithValue("@ch_tipo_mensaje", model.ch_tipo_mensaje);
                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }
        }



        public string GetBody(string empresa, string categoria, string producto, string color1, string color2, string pin, string codcomercio, string fecha, string nrotransaccion, string nroaprobacion, string total, string urlweb, bool bi_valor)
        {
            string bodyRegistrer = "";

            bodyRegistrer += "<html xmlns='http://www.w3.org/1999/xhtml' xmlns:o='urn:schemas-microsoft-com:office:office' style=\"width:100%;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0\">";
            bodyRegistrer += "<head>";
            bodyRegistrer += "<meta charset='UTF-8'>";
            bodyRegistrer += "<meta content='width=device-width, initial-scale=1' name='viewport'>";
            bodyRegistrer += "<meta name='x-apple-disable-message-reformatting'>";
            bodyRegistrer += "<meta http-equiv='X-UA-Compatible' content='IE=edge'>";
            bodyRegistrer += "<meta content='telephone=no' name='format-detection'>";
            bodyRegistrer += "</head>";
            bodyRegistrer += "<body style=\"width:100%;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0\">";
            bodyRegistrer += "<div class='es-wrapper-color' style='background-color:#EEEEEE'>";
            bodyRegistrer += "<table class='es-wrapper' width='100%' cellspacing='0' cellpadding='0' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td valign='top' style='padding:0;Margin:0'>";
            bodyRegistrer += "<table cellpadding='0' cellspacing='0' class='es-content' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='center' style='padding:0;Margin:0'>";
            bodyRegistrer += "<table class='es-content-body' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px' cellspacing='0' cellpadding='0' align='center'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='left' style='Margin:0;padding-left:10px;padding-right:10px;padding-top:15px;padding-bottom:15px'>";
            bodyRegistrer += "<table class='es-left' cellspacing='0' cellpadding='0' align='left' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;float:left'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='left' style='padding:0;Margin:0;width:282px'>";
            bodyRegistrer += "<table width='100%' cellspacing='0' cellpadding='0' role='presentation' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td class='es-infoblock es-m-txt-c' align='left' style='padding:0;Margin:0;line-height:14px;font-size:12px;color:#CCCCCC'><p style='Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:14px;color:#CCCCCC;font-size:12px'></p></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table>";
            bodyRegistrer += "<table class='es-right' cellspacing='0' cellpadding='0' align='right' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;float:right'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='left' style='padding:0;Margin:0;width:278px'>";
            bodyRegistrer += "<table width='100%' cellspacing='0' cellpadding='0' role='presentation' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='right' class='es-infoblock es-m-txt-c' style='padding:0;Margin:0;line-height:14px;font-size:12px;color:#CCCCCC'><p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;line-height:14px;color:#CCCCCC;font-size:12px\"></p></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table>";
            bodyRegistrer += "<table class='es-content' cellspacing='0' cellpadding='0' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'></tr>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='center' style='padding:0;Margin:0'>";
            bodyRegistrer += "<table class='es-header-body' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:" + color1 + ";width:600px' cellspacing='0' cellpadding='0' bgcolor='#044767' align='center'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='left' style='Margin:0;padding-top:35px;padding-bottom:35px;padding-left:35px;padding-right:35px'>";
            bodyRegistrer += "<table class='es-left' cellspacing='0' cellpadding='0' align='left' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;float:left'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td class='es-m-p0r es-m-p20b' valign='top' align='center' style='padding:0;Margin:0;width:340px'>";
            bodyRegistrer += "<table width='100%' cellspacing='0' cellpadding='0' role='presentation' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td class='es-m-txt-c' align='left' style='padding:0;Margin:0'><h1 style=\"Margin:0;line-height:36px;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;font-size:36px;font-style:normal;font-weight:bold;color:#FFFFFF\">" + empresa + "</h1></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table>";
            bodyRegistrer += "<table cellspacing='0' cellpadding='0' align='right' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr class='es-hidden' style='border-collapse:collapse'>";
            bodyRegistrer += "<td class='es-m-p20b' align='left' style='padding:0;Margin:0;width:170px'>";
            bodyRegistrer += "<table width='100%' cellspacing='0' cellpadding='0' role='presentation' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='center' style='padding:0;Margin:0;padding-bottom:5px;font-size:0'>";
            bodyRegistrer += "<table width='100%' height='100%' cellspacing='0' cellpadding='0' border='0' role='presentation' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table>";
            bodyRegistrer += "<table class='es-content' cellspacing='0' cellpadding='0' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='center' style='padding:0;Margin:0'>";
            bodyRegistrer += "<table class='es-content-body' cellspacing='0' cellpadding='0' bgcolor='#ffffff' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;width:600px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='left' style='padding:0;Margin:0;padding-left:35px;padding-right:35px;padding-top:40px'>";
            bodyRegistrer += "<table width='100%' cellspacing='0' cellpadding='0' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td valign='top' align='center' style='padding:0;Margin:0;width:530px'>";
            bodyRegistrer += "<table width='100%' cellspacing='0' cellpadding='0' role='presentation' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='center' style='Margin:0;padding-top:25px;padding-bottom:25px;padding-left:35px;padding-right:35px;font-size:0'><a target='_blank' href='https://viewstripo.email/' style='-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:none;color:#ED8E20;font-size:16px'><img src='https://movilred.sis360.com.pe/Recursos/Img/67611522142640957.png' alt style='display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic' width='120'></a></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='center' style='padding:0;Margin:0;padding-bottom:10px'><h2 style=\"Margin:0;line-height:36px;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;font-size:30px;font-style:normal;font-weight:bold;color:#333333\">¡Tu compra fue existosa!</h2></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='left' style='padding:0;Margin:0;padding-top:15px;padding-bottom:20px'><p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;line-height:24px;color:#777777;font-size:16px\">Realizaste una compra de contenido de: <b>" + categoria + "</b>, para el producto: " + producto + "</p></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table>";
            bodyRegistrer += "<table class='es-content' cellspacing='0' cellpadding='0' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='center' style='padding:0;Margin:0'>";
            bodyRegistrer += "<table class='es-content-body' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:" + color2 + ";width:600px' cellspacing='0' cellpadding='0' bgcolor='#1b9ba3' align='center'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='left' style='Margin:0;padding-top:35px;padding-bottom:35px;padding-left:35px;padding-right:35px'>";
            bodyRegistrer += "<table width='100%' cellspacing='0' cellpadding='0' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td valign='top' align='center' style='padding:0;Margin:0;width:530px'>";
            bodyRegistrer += "<table width='100%' cellspacing='0' cellpadding='0' role='presentation' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='center' style='padding:0;Margin:0'><p style=\"Margin:0;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;font-style:normal;font-weight:bold;color:#FFFFFF\">Tu PIN es:</p></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='center' style='padding:0;Margin:0;padding-top:25px'><h2 style=\"Margin:0;line-height:29px;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;font-size:24px;font-style:normal;font-weight:bold;color:#FFFFFF\">" + pin + "</h2></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='center' style='padding:0;Margin:0;padding-top:25px'><p style=\"Margin:0;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;font-style:normal;font-weight:bold;color:#FFFFFF\">En el comercio identificado con el código: " + codcomercio + "</p></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table>";
            bodyRegistrer += "<table class='es-content' cellspacing='0' cellpadding='0' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='center' style='padding:0;Margin:0'>";
            bodyRegistrer += "<table class='es-content-body' cellspacing='0' cellpadding='0' bgcolor='#ffffff' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;width:600px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='left' style='padding:0;Margin:0;padding-left:35px;padding-right:35px;padding-top:35px'>";
            bodyRegistrer += "<table width='100%' cellspacing='0' cellpadding='0' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td valign='top' align='center' style='padding:0;Margin:0;width:530px'>";
            bodyRegistrer += "<table width='100%' cellspacing='0' cellpadding='0' role='presentation' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='left' style='Margin:0;padding-top:10px;padding-bottom:10px;padding-left:10px;padding-right:10px'>";
            bodyRegistrer += "<table style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;width:500px' class='cke_show_border' cellspacing='1' cellpadding='1' border='0' align='left' role='presentation'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td style='padding:5px 10px 5px 0;Margin:0' width='80%' align='center'><p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;line-height:24px;color:#333333;font-size:16px\">Fecha y hora: " + fecha + "</p></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td style='padding:5px 10px 5px 0;Margin:0' width='80%' align='center'><p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;line-height:24px;color:#333333;font-size:16px\">Número de transacción: " + nrotransaccion + "</p></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td style='padding:5px 10px 5px 0;Margin:0' width='80%' align='center'><p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;line-height:24px;color:#333333;font-size:16px\">Número de aprobación: " + nroaprobacion + "</p></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='center' style='padding:0;Margin:0;padding-top:10px;padding-left:35px;padding-right:35px'>";
            bodyRegistrer += "<table width='100%' cellspacing='0' cellpadding='0' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td valign='top' align='center' style='padding:0;Margin:0;width:530px'>";
            bodyRegistrer += "<table style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;border-top:3px solid #EEEEEE;border-bottom:3px solid #EEEEEE' width='100%' cellspacing='0' cellpadding='0' role='presentation'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='left' style='Margin:0;padding-left:10px;padding-right:10px;padding-top:15px;padding-bottom:15px'>";
            bodyRegistrer += "<table style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;width:500px' class='cke_show_border' cellspacing='1' cellpadding='1' border='0' align='left' role='presentation'>";

            if (bi_valor)
            {
                bodyRegistrer += "<tr style='border-collapse:collapse'>";//
                bodyRegistrer += "<td style='padding:5px 10px 5px 0;Margin:0' width='80%' align='center'><p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;line-height:24px;color:#333333;font-size:16px\"><b>Valor total: " + total + "</b></p></td>";
                bodyRegistrer += "</tr>";//
            }

            bodyRegistrer += "</table>";
            bodyRegistrer += "</td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table>";
            bodyRegistrer += "</td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table>";
            bodyRegistrer += "<table cellpadding='0' cellspacing='0' class='es-footer' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='center' style='padding:0;Margin:0'>";
            bodyRegistrer += "<table class='es-footer-body' cellspacing='0' cellpadding='0' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;width:600px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='left' style='Margin:0;padding-top:35px;padding-left:35px;padding-right:35px;padding-bottom:40px'>";
            bodyRegistrer += "<table width='100%' cellspacing='0' cellpadding='0' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td valign='top' align='center' style='padding:0;Margin:0;width:530px'>";
            bodyRegistrer += "<table width='100%' cellspacing='0' cellpadding='0' role='presentation' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td esdev-links-color='#777777' align='center' class='es-m-txt-c' style='padding:0;Margin:0;padding-bottom:5px'><p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;line-height:21px;color:#777777;font-size:14px\">¿Dudas? Tranquilo, <u><a target='_blank' style='-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:none;color:#777777;font-size:14px' class='unsubscribe' href='" + urlweb + "'>aquí</a></u> estamos para resolverlas.</p></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table>";
            bodyRegistrer += "<table class='es-content' cellspacing='0' cellpadding='0' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='center' style='padding:0;Margin:0'>";
            bodyRegistrer += "<table class='es-content-body' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px' cellspacing='0' cellpadding='0' align='center'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td align='left' style='Margin:0;padding-left:20px;padding-right:20px;padding-top:30px;padding-bottom:30px'>";
            bodyRegistrer += "<table width='100%' cellspacing='0' cellpadding='0' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'>";
            bodyRegistrer += "<tr style='border-collapse:collapse'>";
            bodyRegistrer += "<td valign='top' align='center' style='padding:0;Margin:0;width:560px'></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table></td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table>";
            bodyRegistrer += "</div>";
            bodyRegistrer += "</body>";
            bodyRegistrer += "</html>";

            return bodyRegistrer;
        }

        public string GetBodyFinalAnterior(string empresa, string categoria, string producto, string color1, string color2, string pin, string codcomercio, string fecha, string nrotransaccion, string nroaprobacion, string total, string urlweb, bool bi_valor)
        {
            string bodyRegistrer = "";

            bodyRegistrer += "<html xmlns='http://www.w3.org/1999/xhtml' xmlns:o='urn:schemas-microsoft-com:office:office' style=\"width:100%;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0\">";
            bodyRegistrer += "<head>";
            bodyRegistrer += "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">";
            bodyRegistrer += "</head>";
            bodyRegistrer += "<body bgcolor=\"#FFFFFF\" leftmargin=\"0\" topmargin=\"0\" marginwidth=\"0\" marginheight=\"0\">";
            bodyRegistrer += "<table id=\"Tabla_01\" width=\"751\" height=\"906\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#000000\" align=\"center\" style=\"transform: scale(1, 1) !important;\"> ";
            bodyRegistrer += "<tr style=\"font-size:0px !important;\">";
            bodyRegistrer += "<td colspan=\"7\">";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_01.png\" width=\"751\" height=\"432\" alt=\"\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "<tr style=\"font-size:0px !important;\">";
            bodyRegistrer += "<td colspan=\"2\">";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_02.png\" width=\"109\" height=\"137\" alt=\"\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "<td colspan=\"4\" align=\"center\">";
            bodyRegistrer += "<p style=\"Margin:0; font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height:24px; color:#FFFFFF;Font-size:20px\">Realizaste una compra de contenido de: <b>" + categoria + "</b>, para el producto: " + producto + "</p>";
            bodyRegistrer += "<p style=\"Margin:0; font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height:20px; color:#777777;Font-size:16px\">&nbsp</p>";
            bodyRegistrer += "<p style=\"Margin:0; font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height:12px; color:#777777;Font-size:16px\">En el comercio identificado con el código: " + codcomercio + "</p>";
            bodyRegistrer += "</td>";
            bodyRegistrer += "<td>";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_04.png\" width=\"137\" height=\"137\" alt=\"\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "<tr style=\"font-size:0px !important;\">";
            bodyRegistrer += "<td colspan=\"8\">";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_05.png\" width=\"751\" height=\"70\" alt=\"\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "<tr>";
            bodyRegistrer += "<td rowspan=\"3\">";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_06.png\" width=\"108\" height=\"266\" alt=\"\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "<td colspan=\"2\">";
            bodyRegistrer += "<p style=\"Margin:0; font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height:30px; color:#777777;font-size:12px\">" + fecha + "</p>";
            bodyRegistrer += "</td>";
            bodyRegistrer += "<td colspan=\"2\">";
            bodyRegistrer += "<p style=\"Margin:0; font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height:30px; color:#777777;font-size:12px\">" + nrotransaccion + "</p>";
            bodyRegistrer += "</td>";
            bodyRegistrer += "<td colspan=\"1\">";
            bodyRegistrer += "<p style=\"Margin:0; font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height:30px; color:#777777;font-size:12px\">" + nroaprobacion + "</p>";
            bodyRegistrer += "</td>";
            bodyRegistrer += "<td rowspan=\"3\">";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_10.png\" width=\"137\" height=\"266\" alt=\"\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "<tr>";
            bodyRegistrer += "<td colspan=\"3\" rowspan=\"2\">";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_11.png\" width=\"204\" height=\"236\" alt=\"\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "<td colspan=\"2\" valign=\"bottom\">";
            bodyRegistrer += "<p style=\"Margin:0; font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height:24px; color:#FFFFFF;Font-size:21px\"># PIN :<b> " + pin + "</b></p>";
            bodyRegistrer += "</td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "<tr>";
            bodyRegistrer += "<td colspan=\"2\">";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_13.png\" width=\"302\" height=\"192\" alt=\"\" style=\"padding-top:20px;\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "<tr>";
            bodyRegistrer += "<td>";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/espacio.gif\" width=\"108\" height=\"1\" alt=\"\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "<td>";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/espacio.gif\" width=\"1\" height=\"1\" alt=\"\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "<td>";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/espacio.gif\" width=\"152\" height=\"1\" alt=\"\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "<td>";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/espacio.gif\" width=\"51\" height=\"1\" alt=\"\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "<td>";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/espacio.gif\" width=\"134\" height=\"1\" alt=\"\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "<td>";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/espacio.gif\" width=\"168\" height=\"1\" alt=\"\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "<td>";
            bodyRegistrer += "<img src=\"https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/espacio.gif\" width=\"137\" height=\"1\" alt=\"\">";
            bodyRegistrer += "</td>";
            bodyRegistrer += "</tr>";
            bodyRegistrer += "</table>";
            bodyRegistrer += "</body>";
            bodyRegistrer += "</html>";

            return bodyRegistrer;
        }


        public string GetBodyFinal(string empresa, string categoria, string producto, string color1, string color2, string pin, string codcomercio, string fecha, string nrotransaccion, string nroaprobacion, string total, string urlweb, bool bi_valor)
        {
            string bodyRegistrer = "";

            bodyRegistrer += @"<html";
            bodyRegistrer += @" xmlns=""http://www.w3.org/1999/xhtml""";
            bodyRegistrer += @" xmlns:o=""urn:schemas-microsoft-com:office:office""";
            bodyRegistrer += @" width=""100%""";
            bodyRegistrer += @" style=""width: 100%; font-family: 'open sans', 'helvetica neue', helvetica, arial, sans-serif; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; padding: 0; margin: 0;""";
            bodyRegistrer += @" >";
            bodyRegistrer += @" <head>";
            bodyRegistrer += @" <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />";
            bodyRegistrer += @" </head>";
            bodyRegistrer += @" <style type=""text/css"">";
            bodyRegistrer += @" table {";
            bodyRegistrer += @" border-collapse: collapse;";
            bodyRegistrer += @" border-spacing: 0;";
            bodyRegistrer += @" mso-table-lspace: 0pt !important;";
            bodyRegistrer += @" mso-table-rspace: 0pt !important;";
            bodyRegistrer += @" }";
            bodyRegistrer += @" img {";
            bodyRegistrer += @" display: block;";
            bodyRegistrer += @" }";
            bodyRegistrer += @" </style>";
            bodyRegistrer += @" <body bgcolor=""#FFFFFF"" leftmargin=""0"" topmargin=""0"" marginwidth=""0"" marginheight=""0"">";
            bodyRegistrer += @" <table id=""Tabla_01"" width=""751"" height=""906"" border=""0"" cellpadding=""0"" cellspacing=""0"" bgcolor=""#000000"" align=""center"" style=""border: none; mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-collapse: collapse;"">";
            bodyRegistrer += @" <tbody>";
            bodyRegistrer += @" <tr style=""font-size: 0px !important;"">";
            bodyRegistrer += @" <td colspan=""7"">";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_01.png"" width=""751"" height=""432"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr> ";
            bodyRegistrer += @" <tr style=""font-size: 0px !important;"">";
            bodyRegistrer += @" <td colspan=""2"" width=""109"" height=""137"">";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_02.png"" width=""109"" height=""137"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td colspan=""4"" align=""center"">";
            bodyRegistrer += @" <p style=""margin: 0; font-family: 'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height: 24px; color: #ffffff; font-size: 20px;"">";
            bodyRegistrer += @" Realizaste una compra de contenido de: <b>" + categoria + "</b>, para el producto: " + producto + "";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" <p style=""margin: 0; font-family: 'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height: 20px; color: #777777; font-size: 16px;"">";
            bodyRegistrer += @" &nbsp;";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" <p style=""margin: 0; font-family: 'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height: 12px; color: #777777; font-size: 16px;"">";
            bodyRegistrer += @" En el comercio identificado con el código: " + codcomercio + "";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td>";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_04.png"" width=""137"" height=""137"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" <tr style=""font-size: 0px !important;"">";
            bodyRegistrer += @" <td colspan=""8"" width=""751"" height=""70"">";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_05.png"" width=""751"" height=""70"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" <tr>";
            bodyRegistrer += @" <td rowspan=""3"" width=""108"" height=""266"">";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_06.png"" width=""108"" height=""266"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td colspan=""2"">";
            bodyRegistrer += @" <p style=""margin: 0; font-family: 'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height: 30px; color: #777777; font-size: 12px;"">";
            bodyRegistrer += @" " + fecha + "";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" </td> ";
            bodyRegistrer += @" <td colspan=""2"">";
            bodyRegistrer += @" <p style=""margin: 0; font-family: 'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height: 30px; color: #777777; font-size: 12px;"">";
            bodyRegistrer += @" " + nrotransaccion + "";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td colspan=""1"">";
            bodyRegistrer += @" <p style=""margin: 0; font-family: 'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height: 30px; color: #777777; font-size: 12px;"">";
            bodyRegistrer += @" " + nroaprobacion + "";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td rowspan=""3"" width=""137"" height=""266"">";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_10.png"" width=""137"" height=""266"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr";
            bodyRegistrer += @" <tr>";
            bodyRegistrer += @" <td colspan=""3"" rowspan=""2"" width=""204"" height=""236"">";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_11.png"" width=""204"" height=""236"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td colspan=""2"" align=""bottom"" >";
            bodyRegistrer += @" <p style=""margin: 0; font-family: 'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height: 24px; color: #ffffff; font-size: 21px;""># PIN :<b> " + pin + "</b></p>";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr> ";
            bodyRegistrer += @" <tr>";
            bodyRegistrer += @" <td colspan=""2"" width=""302"" height=""192"" >";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_13.png"" width=""302"" height=""192"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr";
            bodyRegistrer += @" <tr>";
            bodyRegistrer += @" <td width=""108"" height=""1"">";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/espacio.gif"" width=""108"" height=""1"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td width=""1"" height=""1"">";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/espacio.gif"" width=""1"" height=""1"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td width=""152"" height=""1"">";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/espacio.gif"" width=""152"" height=""1"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td width=""51"" height=""1"">";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/espacio.gif"" width=""51"" height=""1"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td width=""134"" height=""1"">";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/espacio.gif"" width=""134"" height=""1"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td width=""168"" height=""1"">";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/espacio.gif"" width=""168"" height=""1"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td width=""137"" height=""1"">";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/espacio.gif"" width=""137"" height=""1"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" </tbody>";
            bodyRegistrer += @" </table>";
            bodyRegistrer += @" </body>";
            bodyRegistrer += @" </html>";




            return bodyRegistrer;
        }


    }
}
