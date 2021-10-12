using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using wa_api_incomm.Models.BanBif;

namespace wa_api_incomm.Services.Contracts
{
    public interface IBanBifService
    {
        object get_datos_banbif(string conexion); 
        //object sel_banbif_rubro_recaudador(string conexion); 
        //object sel_banbif_empresa_rubros(string conexion, RubroModel.Rubro_Input model);
        //object sel_banbif_convenio(string conexion, EmpresaModel.Empresa_Input model);
        //object get_banbif_convenio(string conexion, ConvenioModel.Convenio_Input model);
        object get_deuda(string conexion, DeudaModel.Deuda_Input model);
        object post_pago(string conexion, PagoModel.Pago_Input model);
    }
}
