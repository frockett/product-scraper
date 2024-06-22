﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using product_scraper.Data;

#nullable disable

namespace product_scraper.Migrations
{
    [DbContext(typeof(ScraperContext))]
    [Migration("20240407084941_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.3");

            modelBuilder.Entity("product_scraper.Models.FilterCriteria", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Keywords")
                        .HasColumnType("TEXT");

                    b.Property<int>("MaxPrice")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MinPrice")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("FilterCriteria");
                });

            modelBuilder.Entity("product_scraper.Models.MercariListing", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Price")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("imgUrl")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("MercariListings");
                });
#pragma warning restore 612, 618
        }
    }
}
