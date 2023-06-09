﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TestWebApplication.Data;

#nullable disable

namespace TestWebApplication.Migrations
{
    [DbContext(typeof(WeatherForecastContext))]
    partial class WeatherForecastContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("TestWebApplication.Models.WeatherForecastItem", b =>
                {
                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<int>("TemperatureCAfternoon")
                        .HasColumnType("int");

                    b.Property<int>("TemperatureCEvening")
                        .HasColumnType("int");

                    b.Property<int>("TemperatureCMorning")
                        .HasColumnType("int");

                    b.Property<int>("TemperatureCNight")
                        .HasColumnType("int");

                    b.HasKey("Date");

                    b.ToTable("WeatherForecastItems");
                });
#pragma warning restore 612, 618
        }
    }
}
