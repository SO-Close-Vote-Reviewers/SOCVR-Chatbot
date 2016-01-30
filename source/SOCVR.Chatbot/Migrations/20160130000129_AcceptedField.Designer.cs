using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20160130000129_AcceptedField")]
    partial class AcceptedField
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

            modelBuilder.Entity("SOCVR.Chatbot.Database.PermissionRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool?>("Accepted");

                    b.Property<DateTimeOffset>("RequestedOn");

                    b.Property<int>("RequestedPermissionGroup");

                    b.Property<int>("RequestingUserId");

                    b.Property<int>("ReviewingUserId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("SOCVR.Chatbot.Database.User", b =>
                {
                    b.Property<int>("ProfileId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset?>("LastTrackingPreferenceChange");

                    b.HasKey("ProfileId");
                });

            modelBuilder.Entity("SOCVR.Chatbot.Database.UserPermission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("PermissionGroup");

                    b.Property<int>("UserId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("SOCVR.Chatbot.Database.UserReviewedItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ActionTaken");

                    b.Property<bool?>("AuditPassed");

                    b.Property<string>("PrimaryTag")
                        .IsRequired();

                    b.Property<DateTimeOffset>("ReviewedOn");

                    b.Property<int>("ReviewerId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("SOCVR.Chatbot.Database.PermissionRequest", b =>
                {
                    b.HasOne("SOCVR.Chatbot.Database.User")
                        .WithMany()
                        .HasForeignKey("RequestingUserId");

                    b.HasOne("SOCVR.Chatbot.Database.User")
                        .WithMany()
                        .HasForeignKey("ReviewingUserId");
                });

            modelBuilder.Entity("SOCVR.Chatbot.Database.UserPermission", b =>
                {
                    b.HasOne("SOCVR.Chatbot.Database.User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("SOCVR.Chatbot.Database.UserReviewedItem", b =>
                {
                    b.HasOne("SOCVR.Chatbot.Database.User")
                        .WithMany()
                        .HasForeignKey("ReviewerId");
                });
        }
    }
}
