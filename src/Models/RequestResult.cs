using System;

namespace HappyTokenApi.Models
{
    public class RequestResult
    {
        public object Content { get; set; }

        public int StatusCode { get; set; }

        public RecordData[] Data { get; set; }
    }
}
