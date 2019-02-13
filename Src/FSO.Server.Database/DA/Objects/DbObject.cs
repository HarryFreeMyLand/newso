namespace FSO.Server.Database.DA.Objects
{
    public class DbObject
    {
        public uint object_id { get; set; }
        public int shard_id { get; set; }
        public uint? owner_id { get; set; }
        public int? lot_id { get; set; }
        public string dyn_obj_name { get; set; }
        public uint type { get; set; }
        public ushort graphic { get; set; }
        public uint value { get; set; }
        public int budget { get; set; }
        public ulong dyn_flags_1 { get; set; }
        public ulong dyn_flags_2 { get; set; }
    }
}
