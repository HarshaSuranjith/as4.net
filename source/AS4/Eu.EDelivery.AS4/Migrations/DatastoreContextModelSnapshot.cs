﻿// <auto-generated />
using Eu.EDelivery.AS4.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace Eu.EDelivery.AS4.Migrations
{
    [DbContext(typeof(DatastoreContext))]
    partial class DatastoreContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.InException", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("EbmsRefToMessageId")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("Exception")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<byte[]>("MessageBody")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<string>("Operation")
                        .HasColumnName("Operation")
                        .HasMaxLength(50);

                    b.Property<string>("PMode")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("PModeId")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.HasKey("Id")
                        .HasName("PK_InExceptions");

                    b.HasIndex("EbmsRefToMessageId")
                        .HasName("IX_InExceptions_EbmsRefToMessageId");

                    b.HasIndex("Operation")
                        .HasName("IX_InExceptions_Operation");

                    b.ToTable("InExceptions");
                });

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.InMessage", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Action")
                        .HasMaxLength(255);

                    b.Property<string>("ContentType")
                        .HasMaxLength(256);

                    b.Property<string>("ConversationId")
                        .HasMaxLength(50);

                    b.Property<int>("CurrentRetryCount");

                    b.Property<string>("EbmsMessageId")
                        .HasMaxLength(256);

                    b.Property<string>("EbmsMessageType")
                        .HasMaxLength(50)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("EbmsRefToMessageId")
                        .HasMaxLength(256);

                    b.Property<string>("FromParty")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<bool>("Intermediary");

                    b.Property<bool>("IsDuplicate");

                    b.Property<bool>("IsTest");

                    b.Property<string>("MEP")
                        .HasColumnName("MEP")
                        .HasMaxLength(25)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<int>("MaxRetryCount");

                    b.Property<string>("MessageLocation")
                        .HasMaxLength(512);

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<string>("Mpc")
                        .HasColumnName("MPC")
                        .HasMaxLength(255);

                    b.Property<string>("Operation")
                        .HasColumnName("Operation")
                        .HasMaxLength(50);

                    b.Property<string>("PMode")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("PModeId")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("Service")
                        .HasMaxLength(255);

                    b.Property<string>("SoapEnvelope");

                    b.Property<string>("Status")
                        .HasColumnName("Status")
                        .HasMaxLength(50);

                    b.Property<string>("RetryInterval")
                     .HasMaxLength(50);

                    b.Property<string>("ToParty")
                        .HasMaxLength(255);

                    b.HasKey("Id")
                        .HasName("PK_InMessages");

                    b.HasIndex("EbmsRefToMessageId")
                        .HasName("IX_InMessages_EbmsRefToMessageId");

                    b.HasIndex("EbmsMessageId", "IsDuplicate")
                        .HasName("IX_InMessages_EbmsMessageId_IsDuplicate");

                    b.HasIndex("Operation", "InsertionTime")
                        .HasName("IX_InMessages_Operation_InsertionTime");

                    b.ToTable("InMessages");
                });

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.OutException", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("EbmsRefToMessageId")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("Exception")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<byte[]>("MessageBody")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<string>("Operation")
                        .HasColumnName("Operation")
                        .HasMaxLength(50);

                    b.Property<string>("PMode")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("PModeId")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.HasKey("Id")
                        .HasName("PK_OutExceptions");

                    b.HasIndex("EbmsRefToMessageId")
                        .HasName("IX_OutExceptions_EbmsRefToMessageId");

                    b.HasIndex("Operation")
                        .HasName("IX_OutExceptions_Operation");

                    b.ToTable("OutExceptions");
                });

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.OutMessage", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Action")
                        .HasMaxLength(255);

                    b.Property<string>("ContentType")
                        .HasMaxLength(256);

                    b.Property<string>("ConversationId")
                        .HasMaxLength(50);

                    b.Property<int>("CurrentRetryCount");

                    b.Property<string>("EbmsMessageId")
                        .HasMaxLength(256);

                    b.Property<string>("EbmsMessageType")
                        .HasMaxLength(50)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("EbmsRefToMessageId")
                        .HasMaxLength(256);

                    b.Property<string>("FromParty")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<bool>("Intermediary");

                    b.Property<bool>("IsDuplicate");

                    b.Property<bool>("IsTest");

                    b.Property<string>("MEP")
                        .HasColumnName("MEP")
                        .HasMaxLength(25)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<int>("MaxRetryCount");

                    b.Property<string>("MessageLocation")
                        .HasMaxLength(512);

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<string>("Mpc")
                        .HasColumnName("MPC")
                        .HasMaxLength(255);

                    b.Property<string>("Operation")
                        .HasColumnName("Operation")
                        .HasMaxLength(50);

                    b.Property<string>("PMode")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("PModeId")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("Service")
                        .HasMaxLength(255);

                    b.Property<string>("SoapEnvelope");

                    b.Property<string>("Status")
                        .HasColumnName("Status")
                        .HasMaxLength(50);

                    b.Property<string>("RetryInterval")
                     .HasMaxLength(50);

                    b.Property<string>("ToParty")
                        .HasMaxLength(255);

                    b.HasKey("Id")
                        .HasName("PK_OutMessages");

                    b.HasIndex("EbmsMessageId")
                        .HasName("IX_OutMessages_EbmsMessageId");

                    b.HasIndex("EbmsRefToMessageId")
                        .HasName("IX_OutMessages_EbmsRefToMessageId");

                    b.HasIndex("InsertionTime")
                        .HasName("IX_OutMessages_InsertionTime");

                    b.HasIndex("Operation", "MEP", "Mpc", "InsertionTime")
                        .HasName("IX_OutMessages_Operation_MEP_MPC_InsertionTime");

                    b.ToTable("OutMessages");
                });

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.ReceptionAwareness", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CurrentRetryCount");

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<DateTimeOffset?>("LastSendTime");

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<string>("RefToEbmsMessageId")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<long>("RefToOutMessageId");

                    b.Property<string>("RetryInterval")
                        .HasMaxLength(12);

                    b.Property<string>("Status")
                        .HasMaxLength(25);

                    b.Property<int>("TotalRetryCount");

                    b.HasKey("Id")
                        .HasName("PK_ReceptionAwareness");

                    b.HasAlternateKey("RefToOutMessageId")
                        .HasName("AK_ReceptionAwareness_RefToOutMessageId");

                    b.HasIndex("Status", "CurrentRetryCount")
                        .HasName("IX_ReceptionAwareness_Status_CurrentRetryCount");

                    b.ToTable("ReceptionAwareness");
                });

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.SmpConfiguration", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Action")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("EncryptAlgorithm")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<int>("EncryptAlgorithmKeySize")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("EncryptKeyDigestAlgorithm")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("EncryptKeyMgfAlorithm")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("EncryptKeyTransportAlgorithm")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<byte[]>("EncryptPublicKeyCertificate")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("EncryptPublicKeyCertificateName")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<bool>("EncryptionEnabled")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("FinalRecipient")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("PartyRole")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("PartyType")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("ServiceType")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("ServiceValue")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<bool>("TlsEnabled")
                        .HasColumnName("TLSEnabled")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("ToPartyId")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("Url")
                        .HasColumnName("URL")
                        .HasMaxLength(2083)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.HasKey("Id")
                        .HasName("PK_SmpConfigurations");

                    b.HasIndex("ToPartyId", "PartyRole", "PartyType")
                        .IsUnique()
                        .HasName("IX_SmpConfigurations_ToPartyId_PartyRole_PartyType");

                    b.ToTable("SmpConfigurations");
                });
#pragma warning restore 612, 618
        }
    }
}

