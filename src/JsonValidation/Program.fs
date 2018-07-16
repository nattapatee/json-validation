namespace JsonValidation

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging

module Validator =
    open Newtonsoft.Json
    open NJsonSchema
    open System.ComponentModel.DataAnnotations

    type AppSettings = {
        [<Required>]
        [<JsonProperty("connectionString")>]
        ConnectionString: string

        [<Required>]
        [<JsonProperty("alfresco")>]
        Alfresco: Alfresco
    } and Alfresco =  {
        [<Required>]
        [<JsonProperty("url")>]
        Url: string

        [<Required>]
        [<JsonProperty("password")>]
        Password: string

        [<Required>]
        [<JsonProperty("user")>]
        User: string
    }

    let validateJson json =
        async {
            let! schema = JsonSchema4.FromTypeAsync<AppSettings>() |> Async.AwaitTask
            let errors = schema.Validate(json:string)
            return errors
        } |> Async.RunSynchronously


module Program =
    let exitCode = 0

    let CreateWebHostBuilder args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>();

    [<EntryPoint>]
    let main args =

        let settings = "appsettings.json"
        let json = File.ReadAllText(settings)
        let errors = Validator.validateJson json

        match errors.Count with
        | 0 ->
            CreateWebHostBuilder(args).Build().Run()
            exitCode
        | _ ->
            for item in errors do
                printfn "Error %A %A" (item.Kind) (item.Property)
            -1
