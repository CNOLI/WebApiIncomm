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
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

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
        public void SendMessage(string from, string mensaje, string numero, string vc_aws_access_key_id, string vc_aws_secrect_access_key, string id_trx, string id_trx_hub, SqlConnection cn, ref bool bi_informado)
        {
            try
            {
                AmazonSimpleNotificationServiceClient client =
                       new AmazonSimpleNotificationServiceClient(vc_aws_access_key_id,
                                                                   vc_aws_secrect_access_key,
                                                                   Amazon.RegionEndpoint.USEast1);

                var request = new PublishRequest
                {
                    Message = mensaje,
                    PhoneNumber = numero

                };

                if (from.Length > 11)
                {
                    from = from.Substring(0, 11);
                }

                Dictionary<string, MessageAttributeValue> MessageAttributes = new Dictionary<string, MessageAttributeValue>
                  {
                    {
                      "AWS.SNS.SMS.SenderID", new MessageAttributeValue
                        { DataType = "String", StringValue = from.Replace(" ","-")}
                    }
                  };
                request.MessageAttributes = MessageAttributes;

                client.PublishAsync(request);

                bi_informado = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "idtrx: " + id_trx_hub + " / " + "Envio SMS - " + ex.Message);

                bi_informado = false;
            }
        }
        public void Email(string id_trx, string id_trx_hub, string email, string titulo, string body, string email_envio, string contraseña_email, string smtp_email, int puerto, bool ssl, string empresa, SqlConnection cn, ref  bool bi_informado)
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

                bi_informado = true;

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "idtrx: " + id_trx_hub + " / " + "Envio correo - " + ex.Message);

                bi_informado = false;

                //ReenvioMensajeModel rm = new ReenvioMensajeModel();
                //rm.nu_id_trx = id_trx;
                //rm.ch_tipo_mensaje = "M";
                //var cmd = insSendInfoError(cn, rm);
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

        public string GetBodyIncommSMS(string pin, string url_terminos, string email)
        {
            string bodyRegistrer = "Gracias por tu compra.  Tu PIN es: " + pin + ", enviado también a " + email + ". Términos y condiciones en " + url_terminos;

            return bodyRegistrer;
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


        public string GetBodyIncomm(string empresa, string categoria, string producto, string color1, string color2, string pin, string codcomercio, string fecha, string nrotransaccion, string nroaprobacion, string total, string urlweb, bool bi_valor, string telefono)
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
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" <tr>";

            //bodyRegistrer += @" <td colspan=""3"" rowspan=""2"" width=""204"" height=""236"">";
            //bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_11.png"" width=""204"" height=""236"" alt="""" />";
            //bodyRegistrer += @" </td>";


            bodyRegistrer += @" <td colspan=""3"" rowspan=""2"" width=""204"" height=""236"" style=""vertical-align: top;"">";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_11_2.png"" width=""204"" height=""122"" alt="""" />";
            bodyRegistrer += @" <p style=""margin: 0; font-family: 'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height: 20px; color: #ffffff; font-size: 13px;"">PIN enviado también por mensaje de texto al teléfono " + telefono + " </p>";
            bodyRegistrer += @" <a href=""" + urlweb + @""" target = ""_blank"" style = ""margin: 0; font-family: 'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height: 40px; color: #777777; font-size: 15px;"">Ver términos y condiciones</a></td>";

            bodyRegistrer += @" <td colspan=""2"" align=""bottom"" >";
            bodyRegistrer += @" <p style=""margin: 0; font-family: 'open sans', 'helvetica neue', helvetica, arial, sans-serif; line-height: 24px; color: #ffffff; font-size: 21px;""># PIN :<b> " + pin + "</b></p>";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr> ";
            bodyRegistrer += @" <tr>";
            bodyRegistrer += @" <td colspan=""2"" width=""302"" height=""192"" >";
            bodyRegistrer += @" <img src=""https://movilred.sis360.com.pe/Recursos/Img/Email/POSA/index_13.png"" width=""302"" height=""192"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
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

        public string GetBodyPagos(string vc_desc_servicio, string vc_nombre_comercio, string vc_cod_comercio, string vc_fecha, string vc_nro_transaccion, string vc_empresa, string vc_tipo_identificador, string vc_identificador, string vc_valor, string vc_valor_comision, string vc_valor_total)
        {
            string bodyRegistrer = "";
            bodyRegistrer += @"<html";
            bodyRegistrer += @" xmlns=""http://www.w3.org/1999/xhtml""";
            bodyRegistrer += @" xmlns:o=""urn:schemas-microsoft-com:office:office""";
            bodyRegistrer += @" width=""100%""";
            bodyRegistrer += @" style=""width: 100%; font-family: 'Open Sans', sans-serif; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; padding: 0; margin: 0;""";
            bodyRegistrer += @" >";
            bodyRegistrer += @" <head>";
            bodyRegistrer += @" <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />";
            bodyRegistrer += @" <link rel=""preconnect"" href=""https://fonts.googleapis.com"" />";
            bodyRegistrer += @" <link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin />";
            bodyRegistrer += @" <link href=""https://fonts.googleapis.com/css2?family=Open+Sans:ital,wght@0,300;0,400;0,500;0,600;0,700;0,800;1,800&display=swap"" rel=""stylesheet"" />";
            bodyRegistrer += @" </head>";
            bodyRegistrer += @" <style type=""text/css"">";
            bodyRegistrer += @" table {";
            bodyRegistrer += @" border-collapse: collapse;";
            bodyRegistrer += @" border-spacing: 0px;";
            bodyRegistrer += @" mso-table-lspace: 0pt !important;";
            bodyRegistrer += @" mso-table-rspace: 0pt !important;";
            bodyRegistrer += @" }";
            bodyRegistrer += @" img {";
            bodyRegistrer += @" display: block;";
            bodyRegistrer += @" }";
            bodyRegistrer += @" </style>";
            bodyRegistrer += @" <body bgcolor=""#FFFFFF"" leftmargin=""0"" topmargin=""0"" marginwidth=""0"" marginheight=""0"">";
            bodyRegistrer += @" <table";
            bodyRegistrer += @" id=""Tabla_01""";
            bodyRegistrer += @" width=""834""";
            bodyRegistrer += @" height=""926""";
            bodyRegistrer += @" border=""0""";
            bodyRegistrer += @" cellpadding=""0""";
            bodyRegistrer += @" cellspacing=""0""";
            bodyRegistrer += @" bgcolor=""#896BFF""";
            bodyRegistrer += @" align=""center""";
            bodyRegistrer += @" style=""border: none; mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-collapse: collapse; border-spacing: 0px;""";
            bodyRegistrer += @" >";
            bodyRegistrer += @" <tbody>";
            bodyRegistrer += @" <tr style=""font-size: 0px !important;"">";
            bodyRegistrer += @" <td colspan=""12"" width=""834"" height=""375"">";
            bodyRegistrer += @" <img src=""MPPago_01.jpg"" width=""834"" height=""375"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" <tr style=""font-size: 0px !important;"">";
            bodyRegistrer += @" <td colspan=""7"" width=""262"" height=""152"">";
            bodyRegistrer += @" <img src=""MPCompra_02_01.jpg"" width=""262"" height=""152"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td>";
            bodyRegistrer += @" <p style=""margin: 0; line-height: 30px; color: #fff; font-size: 28px; margin-left: -200; margin-top: -118px; text-align: center; margin-bottom: 0px; font-weight: 600; width: 500px;""> ";
            bodyRegistrer += @" Realizaste un PAGO DE SERVICIO " + vc_desc_servicio + "";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" <p style=""margin: 0; line-height: 30px; color: #fff; font-size: 18px; margin-left: -200px; margin-top: 18px; text-align: center; font-weight: 100; width: 500px;""> ";
            bodyRegistrer += @" En el comercio " + vc_nombre_comercio + " identificado con el código: " + vc_cod_comercio + "";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" <tr>";
            bodyRegistrer += @" <td colspan=""7"" width=""262"" height=""150"">";
            bodyRegistrer += @" <img src=""MPCompra_03_01.jpg"" width=""262"" height=""150"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td>";
            bodyRegistrer += @" <p style=""margin: 0; line-height: 30px; color: #fff; font-size: 18px; margin-left: -175px; margin-top: -30px; margin-bottom: 0px; font-weight: 600;"">";
            bodyRegistrer += @" Fecha y hora:";
            bodyRegistrer += @" <span style=""margin: 0; line-height: 30px; color: #fff; font-size: 18px; margin-top: 18px; font-weight: 100; width: 500px;"">";
            bodyRegistrer += @" " + vc_fecha + "";
            bodyRegistrer += @" </span>";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" <p style=""margin: 0; line-height: 30px; color: #fff; font-size: 18px; margin-left: -175px; margin-bottom: 0px; font-weight: 600;"">";
            bodyRegistrer += @" Número de transacción:";
            bodyRegistrer += @" <span style=""margin: 0; line-height: 30px; color: #fff; font-size: 18px; margin-top: 18px; font-weight: 100; width: 500px;"">";
            bodyRegistrer += @" " + vc_nro_transaccion + "";
            bodyRegistrer += @" </span>";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" <p style=""margin: 0; line-height: 30px; color: #fff; font-size: 18px; margin-bottom: 0px; font-weight: 600; margin-left: -150px; width: 300px; text-align: center;""> ";
            bodyRegistrer += @" Empresa:";
            bodyRegistrer += @" <span style=""margin: 0; line-height: 30px; color: #fff; font-size: 18px; margin-top: 18px; font-weight: 100; width: 500px;"">";
            bodyRegistrer += @" " + vc_empresa + "";
            bodyRegistrer += @" </span>";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" <p style=""margin: 0; line-height: 30px; color: #fff; font-size: 18px; margin-bottom: 0px; font-weight: 600; margin-left: -255px; width: 500px; text-align: center;""> ";
            bodyRegistrer += @" Tipo de identificador:";
            bodyRegistrer += @" <span style=""margin: 0; line-height: 30px; color: #fff; font-size: 18px; margin-top: 18px; font-weight: 100;"">";
            bodyRegistrer += @" " + vc_tipo_identificador + "";
            bodyRegistrer += @" </span>";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" <p style=""margin: 0; line-height: 30px; color: #fff; font-size: 18px; margin-bottom: 0px; font-weight: 600; margin-left: -120px; width: 300px; text-align: center;""> ";
            bodyRegistrer += @" Identificador:";
            bodyRegistrer += @" <span style=""margin: 0; line-height: 30px; color: #fff; font-size: 18px; margin-top: 18px; font-weight: 100; width: 500px;"">";
            bodyRegistrer += @" " + vc_identificador + "";
            bodyRegistrer += @" </span>";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" <p style=""margin: 0; line-height: 30px; color: #fff; font-size: 18px; margin-bottom: 0px; font-weight: 600; margin-left: -280px; width: 600px; text-align: center;""> ";
            bodyRegistrer += @" Valor recaudado:";
            bodyRegistrer += @" <span style=""margin: 0; line-height: 30px; color: #fff; font-size: 18px; margin-top: 18px; font-weight: 100; width: 500px;"">";
            bodyRegistrer += @" S/ " + vc_valor + "";
            bodyRegistrer += @" </span>";
            bodyRegistrer += @" / Comisión: S/ " + vc_valor_comision + "";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" <tr>";
            bodyRegistrer += @" <td colspan=""12"" width=""834"" height=""157"">";
            bodyRegistrer += @" <img src=""MPCompra_04.jpg"" width=""834"" height=""157"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" <tr>";
            bodyRegistrer += @" <td colspan=""1"" width=""120"" height=""82"">";
            bodyRegistrer += @" <img src=""MPCompra_05.jpg"" width=""120"" height=""82"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td colspan=""1"" width=""143"" height=""82"">";
            bodyRegistrer += @" <p style=""line-height: 30px; color: #fff; font-size: 40px; font-weight: 600;"">";
            bodyRegistrer += @" <span> " + vc_valor_total + " </span>";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td colspan=""1"" width=""46"" height=""82"">";
            bodyRegistrer += @" <img src=""MPCompra_06_01.jpg"" width=""46"" height=""82"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td colspan=""8"" width=""525"" height=""82"">";
            bodyRegistrer += @" <img src=""MPCompra_07.jpg"" width=""525"" height=""82"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" <tr>";
            bodyRegistrer += @" <td colspan=""6"" width=""434"" height=""89"">";
            bodyRegistrer += @" <div style=""background-color: #030346; width: 433px; height: 89; margin-left: 1px;"">";
            bodyRegistrer += @" <p style=""line-height: 30px; color: #fff; font-size: 14px; font-weight: 600; margin-left: 90px;"">";
            bodyRegistrer += @" ¿Dudas? Tranquilo,";
            bodyRegistrer += @" <a style=""color: #896bff; text-decoration: underline; cursor: pointer;"" href=""#"">";
            bodyRegistrer += @" aquí";
            bodyRegistrer += @" </a>";
            bodyRegistrer += @" estamos para resolverlas.";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" </div>";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td colspan=""6"" width=""400"" height=""89"">";
            bodyRegistrer += @" <img src=""MPCompra_08_01.png"" width=""400"" height=""89"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr> ";
            bodyRegistrer += @" </tbody>";
            bodyRegistrer += @" </table>";
            bodyRegistrer += @" </body>";
            bodyRegistrer += @"</html>";


            return bodyRegistrer;
        }

        public string GetBodyCompras(string vc_desc_producto, string vc_nombre_comercio, string vc_cod_comercio, string vc_fecha, string vc_nro_transaccion, string vc_celular, string vc_valor)
        {
            string bodyRegistrer = "";

            bodyRegistrer += @"<html";
            bodyRegistrer += @" xmlns=""http://www.w3.org/1999/xhtml""";
            bodyRegistrer += @" xmlns:o=""urn:schemas-microsoft-com:office:office""";
            bodyRegistrer += @" width=""100%""";
            bodyRegistrer += @" style=""width: 100%; font-family: 'Open Sans', sans-serif; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; padding: 0; margin: 0;""";
            bodyRegistrer += @" >";
            bodyRegistrer += @" <head>";
            bodyRegistrer += @" <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />";
            bodyRegistrer += @" <link rel=""preconnect"" href=""https://fonts.googleapis.com"" />";
            bodyRegistrer += @" <link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin />";
            bodyRegistrer += @" <link href=""https://fonts.googleapis.com/css2?family=Open+Sans:ital,wght@0,300;0,400;0,500;0,600;0,700;0,800;1,800&display=swap"" rel=""stylesheet"" />";
            bodyRegistrer += @" </head>";
            bodyRegistrer += @" <style type=""text/css"">";
            bodyRegistrer += @" table {";
            bodyRegistrer += @" border-collapse: collapse;";
            bodyRegistrer += @" border-spacing: 0px;";
            bodyRegistrer += @" mso-table-lspace: 0pt !important;";
            bodyRegistrer += @" mso-table-rspace: 0pt !important;";
            bodyRegistrer += @" }";
            bodyRegistrer += @" img {";
            bodyRegistrer += @" display: block;";
            bodyRegistrer += @" }";
            bodyRegistrer += @" </style>";
            bodyRegistrer += @" <body bgcolor=""#FFFFFF"" leftmargin=""0"" topmargin=""0"" marginwidth=""0"" marginheight=""0"">";
            bodyRegistrer += @" <table";
            bodyRegistrer += @" id=""Tabla_01""";
            bodyRegistrer += @" width=""834""";
            bodyRegistrer += @" height=""926""";
            bodyRegistrer += @" border=""0""";
            bodyRegistrer += @" cellpadding=""0""";
            bodyRegistrer += @" cellspacing=""0""";
            bodyRegistrer += @" bgcolor=""#896BFF""";
            bodyRegistrer += @" align=""center""";
            bodyRegistrer += @" style=""border: none; mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-collapse: collapse; border-spacing: 0px;""";
            bodyRegistrer += @" >";
            bodyRegistrer += @" <tbody>";
            bodyRegistrer += @" <tr style=""font-size: 0px !important;"">";
            bodyRegistrer += @" <td colspan=""12"" width=""834"" height=""375"">";
            bodyRegistrer += @" <img src=""MPCompra_01.jpg"" width=""834"" height=""375"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" <tr style=""font-size: 0px !important;"">";
            bodyRegistrer += @" <td colspan=""7"" width=""262"" height=""152"">";
            bodyRegistrer += @" <img src=""MPCompra_02_01.jpg"" width=""262"" height=""152"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td>";
            bodyRegistrer += @" <p style=""margin: 0; line-height: 30px; color: #fff; font-size: 28px; margin-left: -300; margin-top: -120px; text-align: center; margin-bottom: 4px; font-weight: 600;"">";
            bodyRegistrer += @" Realizaste una Recarga " + vc_desc_producto + "";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" <p style=""margin: 0; line-height: 30px; color: #fff; font-size: 23px; margin-left: -200px; margin-top: 20px; text-align: center; font-weight: 100; width: 500px;"">";
            bodyRegistrer += @" En el comercio " + vc_nombre_comercio + " identificado con el código: " + vc_cod_comercio + "";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" <tr>";
            bodyRegistrer += @" <td colspan=""7"" width=""262"" height=""150"">";
            bodyRegistrer += @" <img src=""MPCompra_03_01.jpg"" width=""262"" height=""150"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td>";
            bodyRegistrer += @" <p style=""margin: 0; line-height: 30px; color: #fff; font-size: 23px; margin-left: -175px; margin-top: -30px; margin-bottom: 4px; font-weight: 600;"">";
            bodyRegistrer += @" Fecha y hora:";
            bodyRegistrer += @" <span style=""margin: 0; line-height: 30px; color: #fff; font-size: 23px; margin-top: 20px; font-weight: 100; width: 500px;"">";
            bodyRegistrer += @" " + vc_fecha + "";
            bodyRegistrer += @" </span>";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" <p style=""margin: 0; line-height: 30px; color: #fff; font-size: 23px; margin-left: -175px; margin-bottom: 4px; font-weight: 600;"">";
            bodyRegistrer += @" Número de transacción:";
            bodyRegistrer += @" <span style=""margin: 0; line-height: 30px; color: #fff; font-size: 23px; margin-top: 20px; font-weight: 100; width: 500px;"">";
            bodyRegistrer += @" " + vc_nro_transaccion + "";
            bodyRegistrer += @" </span>";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" <p style=""margin: 0; line-height: 30px; color: #fff; font-size: 23px; margin-bottom: 4px; font-weight: 600; margin-left: -110px; width: 300px; text-align: center;"">";
            bodyRegistrer += @" Celular:";
            bodyRegistrer += @" <span style=""margin: 0; line-height: 30px; color: #fff; font-size: 23px; margin-top: 20px; font-weight: 100; width: 500px;"">";
            bodyRegistrer += @" " + vc_celular + "";
            bodyRegistrer += @" </span>";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" <tr>";
            bodyRegistrer += @" <td colspan=""12"" width=""834"" height=""157"">";
            bodyRegistrer += @" <img src=""MPCompra_04.jpg"" width=""834"" height=""157"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" <tr>";
            bodyRegistrer += @" <td colspan=""1"" width=""120"" height=""82"">";
            bodyRegistrer += @" <img src=""MPCompra_05.jpg"" width=""120"" height=""82"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td colspan=""1"" width=""143"" height=""82"">";
            bodyRegistrer += @" <p style=""line-height: 30px; color: #fff; font-size: 42px; font-weight: 600;"">";
            bodyRegistrer += @" <span> " + vc_valor + " </span>";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td colspan=""1"" width=""46"" height=""82"">";
            bodyRegistrer += @" <img src=""MPCompra_06_01.jpg"" width=""46"" height=""82"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td colspan=""8"" width=""525"" height=""82"">";
            bodyRegistrer += @" <img src=""MPCompra_07.jpg"" width=""525"" height=""82"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" <tr>";
            bodyRegistrer += @" <td colspan=""6"" width=""434"" height=""89"">";
            bodyRegistrer += @" <div style=""background-color: #030346; width: 433px; height: 89; margin-left: 1px;"">";
            bodyRegistrer += @" <p style=""line-height: 30px; color: #fff; font-size: 14px; font-weight: 600; margin-left: 90px;"">";
            bodyRegistrer += @" ¿Dudas? Tranquilo,";
            bodyRegistrer += @" <a style=""color: #896bff; text-decoration: underline; cursor: pointer;"" href=""#"">";
            bodyRegistrer += @" aquí";
            bodyRegistrer += @" </a>";
            bodyRegistrer += @" estamos para resolverlas.";
            bodyRegistrer += @" </p>";
            bodyRegistrer += @" </div>";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" <td colspan=""6"" width=""400"" height=""89"">";
            bodyRegistrer += @" <img src=""MPCompra_08_01.png"" width=""400"" height=""89"" alt="""" />";
            bodyRegistrer += @" </td>";
            bodyRegistrer += @" </tr>";
            bodyRegistrer += @" </tbody>";
            bodyRegistrer += @" </table>";
            bodyRegistrer += @" </body>";
            bodyRegistrer += @"</html>";


            return bodyRegistrer;
        }

    }
}
