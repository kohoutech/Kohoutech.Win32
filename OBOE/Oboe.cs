﻿/* ----------------------------------------------------------------------------
Kohoutech OBOE Library
Copyright (C) 1998-2020  George E Greaney

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
----------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

//OBOE - Origami Binary for Objects and Executables

namespace Kohoutech.OBOE
{
    public class Oboe
    {
        public const int SECTIONENTRYSIZE = 12;

        //header vals
        public string sig;
        public int extHdrSize;
        public List<Section> sections;

        public Oboe(string name)
        {
            sig = "OBOE";
            extHdrSize = 0;
            sections = new List<Section>();
        }

        public void addSection(Section sec)
        {
            sections.Add(sec);
            sec.num = sections.Count;
        }

        //- writing out to file -----------------------------------------------

        public void writeOboeFile(String outname)
        {
            OutputFile outfile = new OutputFile(outname);
            this.writeOboeFile(outfile);
            outfile.writeOut();
        }

        public void writeOboeFile(OutputFile outfile)
        {
            //write header
            outfile.putFixedString(sig, 4);
            outfile.putFour((uint)extHdrSize);
            outfile.putFour((uint)sections.Count);
            writeExtendedHeader(outfile);

            //write initial section tbl
            uint sectblsize = (uint)(sections.Count * SECTIONENTRYSIZE);
            uint sectbl = outfile.getPos();
            outfile.skip(sectblsize);

            //write section data
            uint pos = outfile.getPos();
            foreach (Section sec in sections)
            {
                sec.addr = pos;
                sec.writeOut(outfile);
                pos = outfile.getPos();
                sec.size = pos - sec.addr;
            }

            //adjust section tbl
            outfile.seek(sectbl);
            foreach (Section sec in sections)
            {
                outfile.putFour(sec.sectype);
                outfile.putFour(sec.addr);
                outfile.putFour(sec.size);
            }
        }

        //for subclasses to add their own specific header fields
        public virtual void writeExtendedHeader(OutputFile outfile)
        {

        }

        public void dumpOboeFile(string dumpname)
        {
            //print out text file report for error checking
            StreamWriter txtout = File.CreateText(dumpname);

            txtout.WriteLine("signature: {0}",sig);
            txtout.WriteLine("extended header size: {0}",extHdrSize.ToString("X4"));
            txtout.WriteLine();
            txtout.WriteLine("section count: {0}", sections.Count.ToString());
            foreach (Section sec in sections)
            {
                txtout.WriteLine("-----------------------------------------");
                sec.dumpSection(txtout);
            }
            txtout.Flush();
            txtout.Close();
        }
    }
}
