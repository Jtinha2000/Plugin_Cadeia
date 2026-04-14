using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JailPlugin.Models
{
    public class CrimeModel
    {
        public string Nome { get; set; }
        public int Multa { get; set; }
        public int TempoDeCustodia { get; set; }
        public CrimeModel()
        {

        }
        public CrimeModel(string nome, int multa, int tempoDeCustodia)
        {
            Nome = nome;
            Multa = multa;
            TempoDeCustodia = tempoDeCustodia;
        }
    }
}
