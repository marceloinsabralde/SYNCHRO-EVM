using Microsoft.EntityFrameworkCore;

namespace SYNCHRO_Perform_NextGen.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
}
