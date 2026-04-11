namespace LazyMapper.Test.Objects;

internal class WorkerArchive
{
   public string ArchiveName { get; set; } = "Lazy Worker Archive";
   
   public Worker Worker { get; set; } = new Worker();
}