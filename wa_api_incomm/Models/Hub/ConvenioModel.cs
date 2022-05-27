using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
	public class ConvenioModel : EntidadBase
	{

		public decimal? nu_id_convenio				{ get; set; }
		public string	vc_cod_convenio				{ get; set; }
		public string	vc_desc_convenio			{ get; set; }
		public int?		nu_id_tipo_moneda_def		{ get; set; }


		public string	vc_nro_celular_aut			{ get; set; }
		public string	vc_clave_aut				{ get; set; }
		public string	vc_clave_encrip_aut			{ get; set; }
		public string	vc_merchant_id				{ get; set; }
		public string	vc_pos_id					{ get; set; }
		public string	vc_source_header			{ get; set; }
		public string	vc_source_body				{ get; set; }
		public string	vc_nro_ip					{ get; set; }


		public string	vc_aws_access_key_id		{ get; set; }
		public string	vc_aws_secrect_access_key	{ get; set; }
		public string	vc_url_web_terminos			{ get; set; }


		public string	vc_desc_empresa				{ get; set; }
		public string	vc_color_header_email		{ get; set; }
		public string	vc_color_body_email			{ get; set; }
		public string	vc_email_envio				{ get; set; }
		public string	vc_password_email			{ get; set; }
		public string	vc_smtp_email				{ get; set; }
		public int?		nu_puerto_smtp_email		{ get; set; }
		public bool?	bi_ssl_email				{ get; set; }
		public string	vc_url_api_aes				{ get; set; }
		public string	vc_clave_aes				{ get; set; }
		public string	vc_nro_telefono_tran_incomm { get; set; }
		public string	vc_nro_complete_incomm		{ get; set; }
		public string	vc_celular_def				{ get; set; }
		public string	vc_email_def				{ get; set; }

		


	}
}
