using System.Collections.Generic;

namespace Компилятор
{
    class Keywords
    {
        Dictionary<byte, Dictionary<string, byte>> kw = new Dictionary<byte, Dictionary<string, byte>>();
        public Dictionary<byte, Dictionary<string, byte>> Kw
        {
            get { return kw; }
        }
        public Keywords()
        {
            Dictionary<string, byte> tmp = new Dictionary<string, byte>();
            tmp["do"] = LexicalAnalyzer.dosy;
            tmp["if"] = LexicalAnalyzer.ifsy;
            tmp["in"] = LexicalAnalyzer.insy;
            tmp["of"] = LexicalAnalyzer.ofsy;
            tmp["or"] = LexicalAnalyzer.orsy;
            tmp["to"] = LexicalAnalyzer.tosy;
            kw[2] = tmp;
            tmp = new Dictionary<string, byte>();
            tmp["end"] = LexicalAnalyzer.endsy;
            tmp["var"] = LexicalAnalyzer.varsy;
            tmp["div"] = LexicalAnalyzer.divsy;
            tmp["and"] = LexicalAnalyzer.andsy;
            tmp["not"] = LexicalAnalyzer.notsy;
            tmp["for"] = LexicalAnalyzer.forsy;
            tmp["mod"] = LexicalAnalyzer.modsy;
            tmp["nil"] = LexicalAnalyzer.nilsy;
            tmp["set"] = LexicalAnalyzer.setsy;
            kw[3] = tmp;
            tmp = new Dictionary<string, byte>();
            tmp["then"] = LexicalAnalyzer.thensy;
            tmp["else"] = LexicalAnalyzer.elsesy;
            tmp["case"] = LexicalAnalyzer.casesy;
            tmp["file"] = LexicalAnalyzer.filesy;
            tmp["goto"] = LexicalAnalyzer.gotosy;
            tmp["type"] = LexicalAnalyzer.typesy;
            tmp["with"] = LexicalAnalyzer.withsy;
            kw[4] = tmp;
            tmp = new Dictionary<string, byte>();
            tmp["begin"] = LexicalAnalyzer.beginsy;
            tmp["while"] = LexicalAnalyzer.whilesy;
            tmp["array"] = LexicalAnalyzer.arraysy;
            tmp["const"] = LexicalAnalyzer.constsy;
            tmp["label"] = LexicalAnalyzer.labelsy;
            tmp["until"] = LexicalAnalyzer.untilsy;
            kw[5] = tmp;
            tmp = new Dictionary<string, byte>();
            tmp["downto"] = LexicalAnalyzer.downtosy;
            tmp["packed"] = LexicalAnalyzer.packedsy;
            tmp["record"] = LexicalAnalyzer.recordsy;
            tmp["repeat"] = LexicalAnalyzer.repeatsy;
            kw[6] = tmp;
            tmp = new Dictionary<string, byte>();
            tmp["program"] = LexicalAnalyzer.programsy;
            kw[7] = tmp;
            tmp = new Dictionary<string, byte>();
            tmp["function"] = LexicalAnalyzer.functionsy;
            kw[8] = tmp;
            tmp = new Dictionary<string, byte>();
            tmp["procedure"] = LexicalAnalyzer.procedurensy;
            kw[9] = tmp;
        }
    }
}