// =================================================================
// 1. USING STATEMENTS
// =================================================================
// Hum yahan un sabhi tools ko import kar rahe hain jinki humein zaroorat padegi.

using IndasApp.API.Services;
using IndasApp.API.Services.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
// =================================================================
// 2. APPLICATION BUILDER SETUP
// =================================================================
// Yeh application ko banane aur configure karne ka starting point hai.

var builder = WebApplication.CreateBuilder(args);

// =================================================================
// 3. DEPENDENCY INJECTION CONTAINER (Services)
// =================================================================
// Yahan hum apni application ko batate hain ki kaun-kaun si services (tools) available hain.

// 3.1. Add Controllers Service
// Yeh ASP.NET Core ko batata hai ki hum API Controllers ka istemaal karenge.
builder.Services.AddControllers()
    .AddNewtonsoftJson(); // This tells ASP.NET Core to use the more flexible 

// 3.2. Add Cookie Authentication Service (Yeh sabse important part hai)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Cookie ka naam jo browser me store hoga.
        options.Cookie.Name = "IndasApp.AuthCookie";
        
        // Cookie sirf HTTPS par hi bheji jayegi. Yeh security ke liye zaroori hai.
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        
        // Cookie ko JavaScript se access nahi kiya ja sakta. Yeh XSS attacks se bachata hai.
        options.Cookie.HttpOnly = true;
        
        // Cookie kitne der tak valid rahegi.
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        
        // Agar user login nahi hai aur ek protected endpoint access karne ki koshish karta hai,
        // to API direct redirect karne ki jagah "401 Unauthorized" status code bhejega.
        // Yeh SPA (Single Page Application) jaise Next.js ke liye zaroori hai.
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
        };
    });

// 3.3. Add Authorization Service
// Yeh [Authorize] attribute ko kaam karne ke liye zaroori hai.
builder.Services.AddAuthorization();

// 3.4. Add CORS Service (Cross-Origin Resource Sharing)
// Yeh hamare Next.js frontend (jo localhost:3000 par chalega) ko
// is backend API (jo alag port par chalega) se baat karne ki permission dega.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJsApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Yahan apne Next.js app ka URL daalein
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Cookies bhejne ke liye yeh zaroori hai
    });
});

// 3.5. Add Custom Application Services
// Yahan hum apni banayi hui services ko register karte hain.
// AddSingleton ka matlab hai ki poori application me PasswordHasher ka sirf ek hi object banega aur use kiya jayega.
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

// AddScoped ka matlab hai ki har incoming HTTP request ke liye AuthService ka ek naya object banega.
// Yeh services ke liye best practice hai jo database se interact karti hain.
builder.Services.AddScoped<IAuthService, AuthService>();


builder.Services.AddScoped<IAttendanceService, AttendanceService>();

builder.Services.AddScoped<IGeofenceService, GeofenceService>();

builder.Services.AddScoped<ITrackingService, TrackingService>();

// =================================================================
// 4. BUILD THE APPLICATION
// =================================================================
// Yahan par saari configured services ko use karke application object banaya jaata hai.

var app = builder.Build();

// =================================================================
// 5. HTTP REQUEST PIPELINE (Middleware)
// =================================================================
// Yahan hum define karte hain ki har incoming HTTP request ko kaise process kiya jayega, step-by-step.
// Order bahut important hai.

// 5.1. Use HTTPS Redirection
// Har HTTP request ko automatically HTTPS par redirect karega.
app.UseHttpsRedirection();

// 5.2. Use CORS
// Hamari banayi hui CORS policy ko apply karega.
app.UseCors("AllowNextJsApp");

// 5.3. Use Authentication
// Har request par check karega ki koi valid cookie aayi hai ya nahi,
// aur agar aayi hai to user ki identity (ClaimsPrincipal) set karega.
app.UseAuthentication();

// 5.4. Use Authorization
// Check karega ki user ko us specific endpoint ko access karne ki permission hai ya nahi.
app.UseAuthorization();

// 5.5. Map Controllers
// Request ko sahi Controller action method tak route karega.
app.MapControllers();

// =================================================================
// 6. RUN THE APPLICATION
// =================================================================
// Application ko start karega aur incoming requests ko sunna shuru karega.

app.Run();

