﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using evtx.Tags;
using NLog;

namespace evtx
{
    public class EventRecord
    {
        public EventRecord(BinaryReader recordData, int recordPosition, long chunkOffset, ChunkInfo chunk)
        {
            var l = LogManager.GetLogger("EventRecord");

            RecordPosition = recordPosition;
            ChunkOffset = chunkOffset;

            recordData.ReadInt32(); //signature

            Size = recordData.ReadUInt32();
            RecordNumber = recordData.ReadInt64();
            Timestamp = DateTimeOffset.FromFileTime(recordData.ReadInt64()).ToUniversalTime();

            if (recordData.PeekChar() != 0xf)
            {
                throw new Exception("Payload does not start with 0x1f!");
            }

            l.Debug(
                $"Chunk: 0x{ChunkOffset:X8} Record position: 0x{RecordPosition:X4} Record #: {RecordNumber.ToString().PadRight(3)} Timestamp: {Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fffffff")}");

            Nodes = new List<IBinXml>();

            while (true)
            {
                var nextTag = TagBuilder.BuildTag(chunkOffset, recordPosition, recordData, chunk);
                
                if (nextTag is TemplateInstance nte)
                {
                    var ms = new MemoryStream(nte.Template.PayloadBytes);
                    var br = new BinaryReader(ms);

                    while (true)
                    {
                        var templateTag = TagBuilder.BuildTag(chunkOffset, recordPosition, br, chunk);

                        Nodes.Add(templateTag);

                        
                    }


                    
                }
                else
                {
                    Nodes.Add(nextTag);    
                }

                if (nextTag is EndOfBXmlStream)
                {
                    //nothing left to do, so exit
                  break;
                }

            }

            l.Debug($"       Event Record node count: {Nodes.Count} ({string.Join(" | ", Nodes)})\r\n");
        }


        public List<IBinXml> Nodes { get; set; }


        public int RecordPosition { get; }
        public long ChunkOffset { get; }

        public uint Size { get; }
        public long RecordNumber { get; }
        public DateTimeOffset Timestamp { get; }

        public string ConvertPayloadToXml()
        {
            var sb = new StringBuilder();

            return sb.ToString();
        }

        public override string ToString()
        {
            return
                $"Chunk: 0x{ChunkOffset:X8} Record position: 0x{RecordPosition:X4} Record #: {RecordNumber.ToString().PadRight(3)} Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss.fffffff}";
        }
    }
}