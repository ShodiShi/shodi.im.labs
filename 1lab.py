import customtkinter as ctk
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg

# Настройки темы
ctk.set_appearance_mode("black")
ctk.set_default_color_theme("blue")

class SimulationApp(ctk.CTk):
    def __init__(self):
        super().__init__()

        self.title("Имитационное моделирование полета тела")
        self.geometry("1150x750")

        # Протокол закрытия через крестик (тот же метод, что и у кнопки)
        self.protocol("WM_DELETE_WINDOW", self.on_closing)

        # Настройка сетки
        self.grid_columnconfigure(1, weight=1)
        self.grid_rowconfigure(0, weight=1)

        # --- Боковая панель (Сайдбар) ---
        self.sidebar = ctk.CTkFrame(self, width=280, corner_radius=0)
        self.sidebar.grid(row=0, column=0, sticky="nsew")
        
        ctk.CTkLabel(self.sidebar, text="НАСТРОЙКИ", font=ctk.CTkFont(size=20, weight="bold")).pack(pady=20)

        self.entries = {}
        fields = [
            ("Нач. скорость (м/с)", "50", "v0"),
            ("Угол броска (град)", "45", "angle"),
            ("Масса тела (кг)", "1.0", "m"),
            ("Коэф. сопротивления", "0.1", "k"),
            ("Базовый шаг dt (с)", "0.1", "dt")
        ]

        for label_text, default, key in fields:
            ctk.CTkLabel(self.sidebar, text=label_text).pack(padx=20, anchor="w")
            entry = ctk.CTkEntry(self.sidebar)
            entry.insert(0, default)
            entry.pack(padx=20, pady=(0, 10), fill="x")
            self.entries[key] = entry

        # Кнопка расчета
        self.btn_calc = ctk.CTkButton(self.sidebar, text="РАССЧИТАТЬ", 
                                     command=self.run_simulation, 
                                     font=ctk.CTkFont(weight="bold"), height=40)
        self.btn_calc.pack(padx=20, pady=20, fill="x")

        # --- Кнопка ВЫХОД (внизу сайдбара) ---
        self.btn_exit = ctk.CTkButton(self.sidebar, text="ВЫХОД", 
                                     command=self.on_closing, 
                                     fg_color="#D32F2F", hover_color="#B71C1C",
                                     font=ctk.CTkFont(weight="bold"), height=40)
        self.btn_exit.pack(padx=20, pady=10, fill="x", side="bottom")

        # --- Контентная часть ---
        self.main_content = ctk.CTkFrame(self, fg_color="transparent")
        self.main_content.grid(row=0, column=1, sticky="nsew", padx=15, pady=15)
        
        # Область графика
        self.fig, self.ax = plt.subplots(figsize=(6, 4), dpi=100)
        self.fig.patch.set_facecolor('#242424') 
        self.ax.set_facecolor('#1a1a1a')
        self.ax.tick_params(colors='white')
        for spine in self.ax.spines.values():
            spine.set_color('white')
        
        self.canvas = FigureCanvasTkAgg(self.fig, master=self.main_content)
        self.canvas.get_tk_widget().pack(fill="both", expand=True, padx=5, pady=5)

        # Текстовое поле для таблицы
        self.stats_box = ctk.CTkTextbox(self.main_content, height=220, font=("Courier New", 14))
        self.stats_box.pack(fill="x", padx=5, pady=5)

    def simulate(self, dt, v0, angle, k, m):
        g = 9.81
        rad = np.radians(angle)
        vx, vy = v0 * np.cos(rad), v0 * np.sin(rad)
        x, y = 0.0, 0.0
        max_h = 0.0
        path_x, path_y = [0.0], [0.0]

        while y >= 0:
            # Ускорение с учетом силы тяжести и вязкого трения
            ax, ay = -(k/m) * vx, -g - (k/m) * vy
            
            # Обновление состояния системы (Метод Эйлера)
            x += vx * dt
            y += vy * dt
            vx += ax * dt
            vy += ay * dt
            
            if y > max_h: max_h = y
            if y >= 0:
                path_x.append(x)
                path_y.append(y)
            if x > 30000: break # Предохранитель
        
        return path_x, path_y, x, max_h, np.sqrt(vx**2 + vy**2)

    def run_simulation(self):
        try:
            v0 = float(self.entries['v0'].get())
            angle = float(self.entries['angle'].get())
            m = float(self.entries['m'].get())
            k = float(self.entries['k'].get())
            base_dt = float(self.entries['dt'].get())

            self.ax.clear()
            self.ax.grid(True, color='#444444', linestyle='--')
            self.ax.set_xlabel("Расстояние (м)", color="white")
            self.ax.set_ylabel("Высота (м)", color="white")
            
            self.stats_box.delete("1.0", "end")
            
            header = f"{'Шаг dt':<12} | {'Дальность':<15} | {'Высота':<12} | {'V конеч.'}\n"
            self.stats_box.insert("end", header)
            self.stats_box.insert("end", "=" * 62 + "\n")

            steps = [base_dt, base_dt/10, base_dt/100, base_dt/1000]
            
            for dt in steps:
                if dt < 1e-7: continue
                px, py, dist, h_max, v_end = self.simulate(dt, v0, angle, k, m)
                self.ax.plot(px, py, label=f"dt={dt:g}")
                row = f"{str(round(dt, 7)):<12} | {dist:<15.3f} | {h_max:<12.3f} | {v_end:.3f}\n"
                self.stats_box.insert("end", row)

            self.ax.legend(facecolor='#1a1a1a', labelcolor='white')
            self.ax.set_title("Исследование сходимости метода Эйлера", color="white", fontsize=14)
            self.canvas.draw()
            
        except ValueError:
            self.stats_box.insert("end", "ОШИБКА: Проверьте числовые данные!")
        except Exception as e:
            self.stats_box.insert("end", f"Ошибка: {e}")

    def on_closing(self):
        """Безопасно останавливает приложение и закрывает окна"""
        self.quit()
        self.destroy()

if __name__ == "__main__":
    app = SimulationApp()
    app.mainloop()