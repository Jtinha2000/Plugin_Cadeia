using JailPlugin.Models;
using Rocket.API;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JailPlugin
{
    public class RoupaPrisioneiro
    {
        public ushort OculosID { get; set; }
        public ushort MascaraID { get; set; }
        public ushort CapaceteID { get; set; }
        public ushort ColeteID { get; set; }
        public ushort CalçaID { get; set; }
        public ushort MochilaID { get; set; }
        public ushort CamisaID { get; set; }
        public RoupaPrisioneiro()
        {
            
        }
        public RoupaPrisioneiro(ushort oculosID, ushort mascaraID, ushort capaceteID, ushort coleteID, ushort calçaID, ushort mochilaID, ushort camisaID)
        {
            OculosID = oculosID;
            MascaraID = mascaraID;
            CapaceteID = capaceteID;
            ColeteID = coleteID;
            CalçaID = calçaID;
            MochilaID = mochilaID;
            CamisaID = camisaID;
        }
    }
    public class Configuration : IRocketPluginConfiguration
    {
        public List<EDeathCause> CausasParaIgnorar { get; set; }
        public List<CrimeModel> Crimes { get; set; }
        public RoupaPrisioneiro Vestimenta { get; set; }
        public List<ushort> ItemsDados { get; set; }
        public int SegundosPorMetal { get; set; }
        public uint DelayParaExibirUIdePreso { get; set; }
        public float RaioMaximoParaAlgemar { get; set; }
        public bool ResetarStatusAoLiberar { get; set; }
        public bool ImpedirPlayerDeEscapar { get; set; }
        public bool ContarTempoOffline { get; set; }
        public bool AlgemarPreso { get; set; }
        public bool TornarPresoImortal { get; set; }
        public string PermissaoDePolicial { get; set; }
        public int CooldownComando190 { get; set; }
        public bool Cancelar190QuandoSemPoliciais { get; set; }
        public void LoadDefaults()
        {
            Crimes = new List<CrimeModel>
            {
                new CrimeModel("Assedio", 500, 10),
                new CrimeModel("Vandalismo", 150, 5),
                new CrimeModel("Posse Ilegal", 1000, 3),
                new CrimeModel("Assalto", 10000, 13),
                new CrimeModel("Homicidio", 15000, 10),
                new CrimeModel("Trafico", 10000, 3600),
                new CrimeModel("Briga", 200, 500),
                new CrimeModel("Desacato", 300, 300),
                new CrimeModel("Lavagem de Dinheiro", 10000, 1800),
                new CrimeModel("Direção Perigosa", 1500, 1200),
            };
            SegundosPorMetal = 10;
            RaioMaximoParaAlgemar = 35f;
            ResetarStatusAoLiberar = true;
            ImpedirPlayerDeEscapar = true;
            Vestimenta = new RoupaPrisioneiro(0, 0, 0, 0, 304, 0, 303);
            ContarTempoOffline = false;
            AlgemarPreso = true;
            TornarPresoImortal = true;
            ItemsDados = new List<ushort> { 1988 };
            DelayParaExibirUIdePreso = 3;
            CooldownComando190 = 60;
            PermissaoDePolicial = "Policial.JailPlugin";
            Cancelar190QuandoSemPoliciais = true;
            CausasParaIgnorar = new List<EDeathCause>();
            for (int i = 0; i <= 29; i++)
                CausasParaIgnorar.Add((EDeathCause)i);
        }
    }
}
