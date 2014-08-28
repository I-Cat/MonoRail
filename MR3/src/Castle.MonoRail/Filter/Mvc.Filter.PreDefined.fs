﻿//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.

namespace Castle.MonoRail

    open System
    open System.ComponentModel.DataAnnotations
    open System.Net
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Hosting.Mvc.Typed
    open Castle.MonoRail.Hosting.Mvc


    /// Experiment: likely to be very naive
    type HandleExceptionAttribute() = 
        inherit FilterAttribute()

        let mutable _excType : Type = null
        let mutable _statusCode = HttpStatusCode.InternalServerError
        let matches (exc) =
            _excType.IsAssignableFrom(exc.GetType())

        member x.HttpStatusCode with get() = _statusCode and set(v) = _statusCode <- v
        member x.ExceptionType  with get() = _excType and set(v) = _excType <- v

        interface IExceptionFilter with
            member x.HandleException(context) = 
                if not context.ExceptionHandled && matches(context.Exception) then
                    context.ActionResult <- HttpResult(_statusCode)
                    context.ExceptionHandled <- true


    /// Experiment: likely to be very naive
    type AccessRoleAttribute() = 
        inherit FilterAttribute()
        let mutable _roles : string[] = [||]

        let is_in_one_of_roles (user:System.Security.Principal.IPrincipal) = 
            _roles |> Array.exists (fun role -> user.IsInRole(role))

        member x.Roles with get() = _roles and set(v) = _roles <- v 

        interface IAuthorizationFilter with
            member x.AuthorizeRequest(context) = 
                let user = context.HttpContext.User
                let cannotAccess = (user = null || not <| is_in_one_of_roles(user))
                
                if cannotAccess then 
                    context.Exception <- System.Web.HttpException(401, "Unauthorized")
                    context.ActionResult <- HttpResult(Net.HttpStatusCode.Unauthorized)


    type ValidateInputAttribute() = 
        inherit FilterAttribute()

        let validate (property: ModelMetadata) (instance: obj) = 
            let validators = property.Validators

            let errs =  validators 
                            |> Seq.map (fun v -> (v, new ValidationContext(instance, null, null, MemberName = property.DisplayName, DisplayName = property.DisplayName)))
                            |> Seq.map (fun (v, ctx) -> v.GetValidationResult(property.GetValue(instance), ctx))
                            |> Seq.filter (fun r -> r <> ValidationResult.Success)
                            |> Seq.toArray

            if errs.Length = 0 then
                (true, String.Empty)
            else
                (false, errs.[0].ErrorMessage)
               

        interface IActionFilter with
            member x.BeforeAction(context:PreActionFilterExecutionContext) =
                let metadataProvider = context.ServiceRegistry.ModelMetadataProvider

                for parm in (context.BindedParameters |> Seq.filter (fun p -> p.Value <> null)) do
                    if (context.ActionResult = null) then
                        let model = metadataProvider.Create(parm.Value.GetType())

                        let validatables = model.Properties.Values 
                                                |> Seq.filter (fun p -> not (Seq.isEmpty p.Validators)) 
                                                |> Seq.map (fun p -> validate p parm.Value) 
                                                |> Seq.filter (fun (r,_) -> not r)
                                                |> Seq.toArray

                        if validatables.Length > 0 then
                            let r, message = validatables.[0]
                            let res = ContentNegotiatedResult<HttpError>(new HttpError(HttpStatusCode.ExpectationFailed, "ValidationError", message))
                            res.StatusCode <- HttpStatusCode.ExpectationFailed
                            context.ActionResult <- res

                ()

            member x.AfterAction(context:AfterActionFilterExecutionContext) = ()
                


