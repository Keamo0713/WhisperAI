using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhisperVaultApi.Data;
using WhisperVaultApi.Models;

namespace WhisperVaultApi.Controllers
{
    public class ConfessionsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly OpenAIClient _openAiClient;
        private readonly string _deploymentName;

        public ConfessionsController(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;

            string endpoint = config["AzureOpenAI:Endpoint"]
                ?? throw new ArgumentNullException(nameof(config), "AzureOpenAI:Endpoint cannot be null");
            string apiKey = config["AzureOpenAI:ApiKey"]
                ?? throw new ArgumentNullException(nameof(config), "AzureOpenAI:ApiKey cannot be null");
            _deploymentName = config["AzureOpenAI:DeploymentName"]
                ?? throw new ArgumentNullException(nameof(config), "AzureOpenAI:DeploymentName cannot be null");

            _openAiClient = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var confessions = await _db.Confessions
                .Where(c => c.IsReleased)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View(confessions);
        }

        [HttpGet]
        public IActionResult Submit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromForm] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                ModelState.AddModelError("Text", "Confession text cannot be empty.");
                return View();
            }

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, "You are a compassionate AI that provides calming, empathetic advice."),
                new ChatMessage(ChatRole.User, text)
            };

            var chatOptions = new ChatCompletionsOptions
            {
                Temperature = 0.7f,
                MaxTokens = 300
            };

            foreach (var msg in messages)
                chatOptions.Messages.Add(msg);

            var response = await _openAiClient.GetChatCompletionsAsync(_deploymentName, chatOptions);
            var aiReply = response.Value.Choices.FirstOrDefault()?.Message?.Content ?? "Sorry, I couldn't generate a response.";

            ViewBag.AiResponse = aiReply;
            ViewBag.Text = text;

            return View("SubmitResult");
        }

        [HttpPost]
        public async Task<IActionResult> ReleaseAfterAdvice([FromForm] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return RedirectToAction("Submit");

            var confession = new Confession
            {
                Id = Guid.NewGuid(),
                Text = text,
                SubmittedAt = DateTime.UtcNow,
                IsReleased = true
            };

            _db.Confessions.Add(confession);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
