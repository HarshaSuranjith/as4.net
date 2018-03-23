﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Eu.EDelivery.AS4.Migrations
{
    public partial class AddSmpConfigurationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SmpConfigurations",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<string>(maxLength: 256, nullable: true),
                    EncryptAlgorithm = table.Column<string>(maxLength: 256, nullable: true),
                    EncryptAlgorithmKeySize = table.Column<int>(nullable: false),
                    EncryptKeyDigestAlgorithm = table.Column<string>(maxLength: 256, nullable: true),
                    EncryptKeyMgfAlorithm = table.Column<string>(nullable: true),
                    EncryptKeyTransportAlgorithm = table.Column<string>(maxLength: 256, nullable: true),
                    EncryptPublicKeyCertificate = table.Column<byte[]>(maxLength: int.MaxValue, nullable: true),
                    EncryptPublicKeyCertificateName = table.Column<string>(maxLength: 256, nullable: true),
                    EncryptionEnabled = table.Column<bool>(nullable: false),
                    FinalRecipient = table.Column<string>(maxLength: 256, nullable: true),
                    PartyRole = table.Column<string>(maxLength: 256, nullable: false),
                    PartyType = table.Column<string>(maxLength: 256, nullable: false),
                    ServiceType = table.Column<string>(maxLength: 256, nullable: true),
                    ServiceValue = table.Column<string>(maxLength: 256, nullable: true),
                    TLSEnabled = table.Column<bool>(nullable: false),
                    ToPartyId = table.Column<string>(maxLength: 256, nullable: false),
                    URL = table.Column<string>(maxLength: 2083, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmpConfigurations", x => x.Id);
                    table.UniqueConstraint("AK_SmpConfigurations_ToPartyId_PartyRole_PartyType", x => new { x.ToPartyId, x.PartyRole, x.PartyType });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmpConfigurations");
        }
    }
}
