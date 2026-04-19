namespace ConfigEditor
{
    public class NodeData
    {
        public string NodeType { get; set; }
        public object Data { get; set; }
        public Type ItemType { get; set; }

        public NodeData(string nodeType, object data, Type itemType)
        {
            NodeType = nodeType;
            Data = data;
            ItemType = itemType;
        }
    }
}
