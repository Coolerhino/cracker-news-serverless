namespace ServerlessDemo.Models
{
    public static class Mappings
    {
        public static CrackerTableEntity ToTableEntity(this Cracker cracker)
        {
            return new CrackerTableEntity()
            {
                PartitionKey = "CRACKER",
                RowKey = cracker.Id,
                CreatedTime = cracker.Created,
                Description = cracker.Description,
                IsRead = cracker.IsRead
            };
        }
        public static Cracker ToCracker(this CrackerTableEntity cracker)
        {
            return new Cracker()
            {
                Id = cracker.RowKey,
                Created = cracker.CreatedTime,
                Description = cracker.Description,
                IsRead = cracker.IsRead
            };
        }

    }
}