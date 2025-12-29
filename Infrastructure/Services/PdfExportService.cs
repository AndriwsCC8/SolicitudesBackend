using Application.DTOs.Solicitudes;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace Infrastructure.Services
{
    public interface IPdfExportService
    {
        byte[] GenerarPdfSolicitud(SolicitudDto solicitud);
        byte[] GenerarPngSolicitud(SolicitudDto solicitud);
    }

    public class PdfExportService : IPdfExportService
    {
        public PdfExportService()
        {
            // Configurar licencia de QuestPDF para uso comunitario
            // Para empresas con ingresos < $1M USD al año
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerarPdfSolicitud(SolicitudDto solicitud)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header()
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(10)
                        .Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("SISTEMA DE GESTIÓN DE SOLICITUDES")
                                    .FontSize(16)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                column.Item().Text($"Solicitud: {solicitud.Numero}")
                                    .FontSize(14)
                                    .SemiBold()
                                    .FontColor(Colors.Grey.Darken2);
                            });

                            row.ConstantItem(100).AlignRight().Column(column =>
                            {
                                column.Item().Text(ObtenerEstadoTexto(solicitud.Estado))
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(ObtenerColorEstado(solicitud.Estado));
                                
                                column.Item().Text($"Prioridad: {solicitud.Prioridad}")
                                    .FontSize(10)
                                    .FontColor(ObtenerColorPrioridad(solicitud.Prioridad));
                            });
                        });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            // Información General
                            column.Item().Element(SeccionInformacionGeneral);

                            // Datos del Solicitante
                            column.Item().Element(SeccionSolicitante);

                            // Descripción
                            column.Item().Element(SeccionDescripcion);

                            // Información Adicional
                            column.Item().Element(SeccionInformacionAdicional);

                            if (!string.IsNullOrEmpty(solicitud.MotivoRechazo))
                            {
                                column.Item().Element(SeccionMotivoRechazo);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generado el: ");
                            x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).SemiBold();
                        });
                });

                void SeccionInformacionGeneral(IContainer container)
                {
                    container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(column =>
                    {
                        column.Item().Text("INFORMACIÓN GENERAL").FontSize(13).Bold().FontColor(Colors.Blue.Darken1);
                        column.Item().PaddingTop(10);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(text =>
                                {
                                    text.Span("Código: ").SemiBold();
                                    text.Span(solicitud.Numero);
                                });
                                col.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("Tipo: ").SemiBold();
                                    text.Span(solicitud.TipoSolicitud);
                                });
                                col.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("Área: ").SemiBold();
                                    text.Span(solicitud.Area);
                                });
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(text =>
                                {
                                    text.Span("Estado: ").SemiBold();
                                    text.Span(solicitud.Estado).FontColor(ObtenerColorEstado(solicitud.Estado));
                                });
                                col.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("Prioridad: ").SemiBold();
                                    text.Span(solicitud.Prioridad).FontColor(ObtenerColorPrioridad(solicitud.Prioridad));
                                });
                                col.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("Fecha de Creación: ").SemiBold();
                                    text.Span(solicitud.FechaCreacion.ToString("dd/MM/yyyy HH:mm"));
                                });
                            });
                        });
                    });
                }

                void SeccionSolicitante(IContainer container)
                {
                    container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(column =>
                    {
                        column.Item().Text("DATOS DEL SOLICITANTE").FontSize(13).Bold().FontColor(Colors.Blue.Darken1);
                        column.Item().PaddingTop(10);

                        column.Item().Text(text =>
                        {
                            text.Span("Nombre: ").SemiBold();
                            text.Span(solicitud.Solicitante);
                        });

                        column.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span("Email: ").SemiBold();
                            text.Span(solicitud.SolicitanteEmail);
                        });

                        if (solicitud.GestorAsignadoId.HasValue)
                        {
                            column.Item().PaddingTop(10).Text(text =>
                            {
                                text.Span("Gestor Asignado: ").SemiBold();
                                text.Span(solicitud.GestorAsignado ?? "No asignado");
                            });

                            if (!string.IsNullOrEmpty(solicitud.GestorAsignadoEmail))
                            {
                                column.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("Email Gestor: ").SemiBold();
                                    text.Span(solicitud.GestorAsignadoEmail);
                                });
                            }
                        }
                    });
                }

                void SeccionDescripcion(IContainer container)
                {
                    container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(column =>
                    {
                        column.Item().Text("ASUNTO").FontSize(13).Bold().FontColor(Colors.Blue.Darken1);
                        column.Item().PaddingTop(5).Text(solicitud.Asunto).FontSize(12).SemiBold();

                        column.Item().PaddingTop(15).Text("DESCRIPCIÓN").FontSize(13).Bold().FontColor(Colors.Blue.Darken1);
                        column.Item().PaddingTop(5).Text(solicitud.Descripcion).FontSize(11).LineHeight(1.5f);
                    });
                }

                void SeccionInformacionAdicional(IContainer container)
                {
                    container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(column =>
                    {
                        column.Item().Text("INFORMACIÓN ADICIONAL").FontSize(13).Bold().FontColor(Colors.Blue.Darken1);
                        column.Item().PaddingTop(10);

                        if (solicitud.FechaCierre.HasValue)
                        {
                            column.Item().Text(text =>
                            {
                                text.Span("Fecha de Cierre: ").SemiBold();
                                text.Span(solicitud.FechaCierre.Value.ToString("dd/MM/yyyy HH:mm"));
                            });
                        }

                        column.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span("ID Solicitud: ").SemiBold();
                            text.Span(solicitud.Id.ToString());
                        });
                    });
                }

                void SeccionMotivoRechazo(IContainer container)
                {
                    container.Border(1).BorderColor(Colors.Red.Lighten3).Background(Colors.Red.Lighten5).Padding(15).Column(column =>
                    {
                        column.Item().Text("MOTIVO DE RECHAZO").FontSize(13).Bold().FontColor(Colors.Red.Darken2);
                        column.Item().PaddingTop(5).Text(solicitud.MotivoRechazo).FontSize(11).LineHeight(1.5f);
                    });
                }
            });

            return document.GeneratePdf();
        }

        private string ObtenerEstadoTexto(string estado)
        {
            return estado switch
            {
                "Nueva" => "NUEVA",
                "EnProceso" => "EN PROCESO",
                "Resuelta" => "RESUELTA",
                "Cerrada" => "CERRADA",
                "Rechazada" => "RECHAZADA",
                _ => estado.ToUpper()
            };
        }

        private string ObtenerColorEstado(string estado)
        {
            return estado switch
            {
                "Nueva" => Colors.Blue.Medium,
                "EnProceso" => Colors.Orange.Medium,
                "Resuelta" => Colors.Green.Medium,
                "Cerrada" => Colors.Grey.Darken1,
                "Rechazada" => Colors.Red.Medium,
                _ => Colors.Black
            };
        }

        private string ObtenerColorPrioridad(string prioridad)
        {
            return prioridad switch
            {
                "Alta" => Colors.Red.Medium,
                "Media" => Colors.Orange.Medium,
                "Baja" => Colors.Green.Medium,
                _ => Colors.Black
            };
        }

        public byte[] GenerarPngSolicitud(SolicitudDto solicitud)
        {
            // Configurar dimensiones (A4 en pixels a 300 DPI)
            int width = 2480;  // 210mm * 300 DPI / 25.4
            int height = 3508; // 297mm * 300 DPI / 25.4

            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);

            // Fondo blanco
            canvas.Clear(SKColors.White);

            // Renderizar texto e información de la solicitud directamente
            RenderizarSolicitudEnCanvas(canvas, solicitud, width, height);

            // Codificar a PNG
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        private void RenderizarSolicitudEnCanvas(SKCanvas canvas, SolicitudDto solicitud, int width, int height)
        {
            var paint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.Black
            };
            var font = new SKFont { Size = 32 };

            var paintHeader = new SKPaint
            {
                IsAntialias = true,
                Color = SKColor.Parse("#1976D2")
            };
            var fontHeader = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 48);

            var paintSubtitle = new SKPaint
            {
                IsAntialias = true,
                Color = SKColor.Parse("#424242")
            };
            var fontSubtitle = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 40);

            var paintLabel = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.Black
            };
            var fontLabel = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 32);

            var paintValue = new SKPaint
            {
                IsAntialias = true,
                Color = SKColor.Parse("#424242")
            };
            var fontValue = new SKFont { Size = 32 };

            float y = 100;
            float margin = 80;

            // Encabezado
            canvas.DrawText("SISTEMA DE GESTIÓN DE SOLICITUDES", margin, y, fontHeader, paintHeader);
            y += 70;
            canvas.DrawText($"Solicitud: {solicitud.Numero}", margin, y, fontSubtitle, paintSubtitle);
            y += 60;

            // Estado y Prioridad
            var estadoPaint = new SKPaint
            {
                IsAntialias = true,
                Color = ObtenerSKColorEstado(solicitud.Estado)
            };
            var fontEstado = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 36);
            canvas.DrawText(ObtenerEstadoTexto(solicitud.Estado), margin, y, fontEstado, estadoPaint);
            
            var prioridadPaint = new SKPaint
            {
                IsAntialias = true,
                Color = ObtenerSKColorPrioridad(solicitud.Prioridad)
            };
            var fontPrioridad = new SKFont { Size = 30 };
            canvas.DrawText($"Prioridad: {solicitud.Prioridad}", margin + 400, y, fontPrioridad, prioridadPaint);
            y += 80;

            // Línea separadora
            var linePaint = new SKPaint
            {
                Color = SKColor.Parse("#E0E0E0"),
                StrokeWidth = 2
            };
            canvas.DrawLine(margin, y, width - margin, y, linePaint);
            y += 60;

            // INFORMACIÓN GENERAL
            canvas.DrawText("INFORMACIÓN GENERAL", margin, y, fontSubtitle, paintSubtitle);
            y += 60;

            canvas.DrawText("Código: ", margin, y, fontLabel, paintLabel);
            canvas.DrawText(solicitud.Numero, margin + 200, y, fontValue, paintValue);
            y += 50;

            canvas.DrawText("Tipo: ", margin, y, fontLabel, paintLabel);
            canvas.DrawText(solicitud.TipoSolicitud, margin + 200, y, fontValue, paintValue);
            y += 50;

            canvas.DrawText("Área: ", margin, y, fontLabel, paintLabel);
            canvas.DrawText(solicitud.Area, margin + 200, y, fontValue, paintValue);
            y += 50;

            canvas.DrawText("Fecha: ", margin, y, fontLabel, paintLabel);
            canvas.DrawText(solicitud.FechaCreacion.ToString("dd/MM/yyyy HH:mm"), margin + 200, y, fontValue, paintValue);
            y += 80;

            // DATOS DEL SOLICITANTE
            canvas.DrawText("DATOS DEL SOLICITANTE", margin, y, fontSubtitle, paintSubtitle);
            y += 60;

            canvas.DrawText("Nombre: ", margin, y, fontLabel, paintLabel);
            canvas.DrawText(solicitud.Solicitante, margin + 200, y, fontValue, paintValue);
            y += 50;

            canvas.DrawText("Email: ", margin, y, fontLabel, paintLabel);
            canvas.DrawText(solicitud.SolicitanteEmail, margin + 200, y, fontValue, paintValue);
            y += 50;

            if (solicitud.GestorAsignadoId.HasValue)
            {
                canvas.DrawText("Gestor: ", margin, y, fontLabel, paintLabel);
                canvas.DrawText(solicitud.GestorAsignado ?? "No asignado", margin + 200, y, fontValue, paintValue);
                y += 50;
            }

            y += 30;

            // ASUNTO Y DESCRIPCIÓN
            canvas.DrawText("ASUNTO", margin, y, fontSubtitle, paintSubtitle);
            y += 60;
            
            var asuntoPaint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.Black
            };
            var fontAsunto = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 34);
            canvas.DrawText(solicitud.Asunto, margin, y, fontAsunto, asuntoPaint);
            y += 80;

            canvas.DrawText("DESCRIPCIÓN", margin, y, fontSubtitle, paintSubtitle);
            y += 60;

            // Dividir descripción en líneas
            var descripcionLineas = DividirTextoEnLineas(solicitud.Descripcion, 2200, fontValue);
            foreach (var linea in descripcionLineas)
            {
                canvas.DrawText(linea, margin, y, fontValue, paintValue);
                y += 45;
                if (y > height - 200) break; // Evitar salir de la página
            }

            y += 40;

            // MOTIVO RECHAZO si existe
            if (!string.IsNullOrEmpty(solicitud.MotivoRechazo))
            {
                var rechazoHeaderPaint = new SKPaint
                {
                    IsAntialias = true,
                    Color = SKColor.Parse("#D32F2F")
                };
                var fontRechazoHeader = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 36);
                canvas.DrawText("MOTIVO DE RECHAZO", margin, y, fontRechazoHeader, rechazoHeaderPaint);
                y += 60;

                var rechazoTextPaint = new SKPaint
                {
                    IsAntialias = true,
                    Color = SKColor.Parse("#D32F2F")
                };
                var fontRechazo = new SKFont { Size = 32 };
                var motivoLineas = DividirTextoEnLineas(solicitud.MotivoRechazo, 2200, fontRechazo);
                foreach (var linea in motivoLineas)
                {
                    canvas.DrawText(linea, margin, y, fontRechazo, rechazoTextPaint);
                    y += 45;
                    if (y > height - 200) break;
                }
            }

            // Footer
            var footerPaint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColor.Parse("#757575")
            };
            var fontFooter = new SKFont { Size = 28 };
            canvas.DrawText($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}", margin, height - 80, fontFooter, footerPaint);
        }

        private List<string> DividirTextoEnLineas(string texto, float maxWidth, SKFont font)
        {
            var lineas = new List<string>();
            var palabras = texto.Split(' ');
            var lineaActual = "";

            foreach (var palabra in palabras)
            {
                var lineaPrueba = string.IsNullOrEmpty(lineaActual) ? palabra : lineaActual + " " + palabra;
                var ancho = font.MeasureText(lineaPrueba);

                if (ancho > maxWidth && !string.IsNullOrEmpty(lineaActual))
                {
                    lineas.Add(lineaActual);
                    lineaActual = palabra;
                }
                else
                {
                    lineaActual = lineaPrueba;
                }
            }

            if (!string.IsNullOrEmpty(lineaActual))
            {
                lineas.Add(lineaActual);
            }

            return lineas;
        }

        private SKColor ObtenerSKColorEstado(string estado)
        {
            return estado switch
            {
                "Nueva" => SKColor.Parse("#2196F3"),
                "EnProceso" => SKColor.Parse("#FF9800"),
                "Resuelta" => SKColor.Parse("#4CAF50"),
                "Cerrada" => SKColor.Parse("#757575"),
                "Rechazada" => SKColor.Parse("#F44336"),
                _ => SKColors.Black
            };
        }

        private SKColor ObtenerSKColorPrioridad(string prioridad)
        {
            return prioridad switch
            {
                "Alta" => SKColor.Parse("#F44336"),
                "Media" => SKColor.Parse("#FF9800"),
                "Baja" => SKColor.Parse("#4CAF50"),
                _ => SKColors.Black
            };
        }
    }
}
