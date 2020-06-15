using Common.ModelInterfaces;

namespace Common.Models
{
    public class SequenceId : IIdentifier
    {
        public string Id { get; set; }

        public string Subject { get; set; }

        public long Sequence { get; set; }
    }
}
