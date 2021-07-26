using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities
{
    public class UpdateAttendance
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            public DataContext Context { get; }
            public IUserAccessor UserAccessor { get; }

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                Context = context;
                UserAccessor = userAccessor;
            }
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                //Gets the correct activity with a GUID
                var activity = await Context.Activities
                    .Include(a => a.Attendees)
                    .ThenInclude(u => u.AppUser)
                    .SingleOrDefaultAsync(x => x.Id == request.Id);
                // Returns BadRequest
                if (activity == null) return null;
                // Get the calling users username, through IUserAccessor.
                var user = await Context.Users.FirstOrDefaultAsync(x => x.UserName == UserAccessor.GetUsername());
                // Returns BadRequest
                if (user == null) return null;
                // The host of the activity
                var hostUsername = activity.Attendees.FirstOrDefault(x => x.IsHost)?.AppUser?.UserName;
                // Get the attendance
                var attendance = activity.Attendees.FirstOrDefault(x => x.AppUser.UserName == user.UserName);
                // If the caller is host, either cancel or reopen activity.
                if (attendance != null && hostUsername == user.UserName)
                {
                    activity.IsCancelled = !activity.IsCancelled;
                }
                // If caller is not host but attending, remove from activity.
                if (attendance != null && hostUsername != user.UserName)
                {
                    activity.Attendees.Remove(attendance);
                }
                // If caller has not attended, add him to the activity.
                if (attendance == null)
                {
                    attendance = new ActivityAttendee()
                    {
                        AppUser = user,
                        Activity = activity,
                        IsHost = false
                    };
                    activity.Attendees.Add(attendance);
                }
                
                var results = await Context.SaveChangesAsync() > 0;
                // Check if inserting was successfull.
                return results ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Problem updating attendance");
            }
        }
    }
}