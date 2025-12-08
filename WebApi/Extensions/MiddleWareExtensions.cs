namespace WebApi.Extensions
{
    public static class MiddleWareExtensions
    {
        public static WebApplication UseDeveloperFeatures(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
            return app;
        }

        public static WebApplication UseAppRequestPipeline(this WebApplication app, string corsPolicy)
        {
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(corsPolicy);
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            return app;
        }
    }
}
