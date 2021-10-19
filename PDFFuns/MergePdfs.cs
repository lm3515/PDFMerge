using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace PrintPDFs.PDFFuns
{

    public static class MergePdfs
    {
        public static int MergePdfFun(string outputPath, List<string> inputPathList)
        {
            if (inputPathList.Count == 0)
            {
                return 1;
            }
            PdfReader reader = null;
            Document sourceDocument = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage;

            sourceDocument = new Document();
            pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPath, System.IO.FileMode.Create));

            //output file Open  
            sourceDocument.Open();

            //files list wise Loop  
            for (int f = 0; f < inputPathList.Count; f++)
            {
                int pages = TotalPageCount(inputPathList[f]);

                reader = new PdfReader(inputPathList[f]);
                //Add pages in new file  
                for (int i = 1; i <= pages; i++)
                {
                    importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                    pdfCopyProvider.AddPage(importedPage);
                }

                reader.Close();
            }

            //save the output file  
            sourceDocument.Close();
            return 0;
        }

        private static int TotalPageCount(string file)
        {
            using (StreamReader sr = new StreamReader(System.IO.File.OpenRead(file)))
            {
                Regex regex = new Regex(@"/Type\s*/Page[^s]");
                MatchCollection matches = regex.Matches(sr.ReadToEnd());

                return matches.Count;
            }
        }
    }
}
