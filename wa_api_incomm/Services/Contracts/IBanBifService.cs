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
        object sel_rubro_recaudador(string conexion); 
        object sel_empresa_rubros(string conexion, RubroModel model);
        object sel_convenio(string conexion, EmpresaModel model);
        object get_convenio(string conexion, ConvenioModel model);
    }
}
