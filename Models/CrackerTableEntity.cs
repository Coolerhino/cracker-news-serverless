using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace ServerlessDemo.Models
{
    public class CrackerTableEntity : TableEntity
    {
        public DateTime CreatedTime { get; set; }
        public string Description { get; set; }
        public bool IsRead { get; set; }
    }
}