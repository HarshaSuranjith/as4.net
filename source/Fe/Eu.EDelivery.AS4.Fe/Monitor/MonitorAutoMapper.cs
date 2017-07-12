﻿using AutoMapper;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Fe.Monitor.Model;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    /// <summary>
    /// Setup automapper for the monitor
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class MonitorAutoMapper : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonitorAutoMapper"/> class.
        /// </summary>
        public MonitorAutoMapper()
        {
            CreateMap<InMessage, Message>()
                .ForMember(x => x.Status, x => x.MapFrom(y => y.StatusString))
                .ForMember(x => x.ExceptionType, x => x.MapFrom(y => y.ExceptionTypeString))
                .ForMember(x => x.EbmsMessageType, x => x.MapFrom(y => y.EbmsMessageTypeString))
                .ForMember(x => x.Operation, x => x.MapFrom(y => y.OperationString))
                .ForMember(x => x.ContentType, x => x.MapFrom(y => y.SimplifyContentType()))
                .ForMember(x => x.Direction, x => x.UseValue(Direction.Inbound));
            CreateMap<OutMessage, Message>()
                .ForMember(x => x.Status, x => x.MapFrom(y => y.StatusString))
                .ForMember(x => x.ExceptionType, x => x.MapFrom(y => y.ExceptionTypeString))
                .ForMember(x => x.EbmsMessageType, x => x.MapFrom(y => y.EbmsMessageTypeString))
                .ForMember(x => x.Operation, x => x.MapFrom(y => y.OperationString))
                .ForMember(x => x.ContentType, x => x.MapFrom(y => y.SimplifyContentType()))
                .ForMember(x => x.Direction, x => x.UseValue(Direction.Outbound));
            CreateMap<InException, ExceptionMessage>()
              .ForMember(x => x.Direction, x => x.UseValue(Direction.Inbound));
            CreateMap<OutException, ExceptionMessage>()
              .ForMember(x => x.Direction, x => x.UseValue(Direction.Outbound));
            //CreateMap<ExceptionEntity, ExceptionMessage>()
            //    .ForMember(x => x.Operation, x => x.MapFrom(y => y.OperationString));
            //CreateMap<MessageEntity, Message>()
            //  .ForMember(x => x.Status, x => x.MapFrom(y => y.StatusString))
            //  .ForMember(x => x.ContentType, x => x.MapFrom(y => y.SimplifyContentType()));
        }
    }
}