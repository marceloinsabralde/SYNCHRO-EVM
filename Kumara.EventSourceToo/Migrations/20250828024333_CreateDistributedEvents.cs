using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kumara.EventSourceToo.Migrations
{
    /// <inheritdoc />
    public partial class CreateDistributedEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SELECT create_distributed_table('events', 'account_id')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SELECT undistribute_table('events')");
        }
    }
}
