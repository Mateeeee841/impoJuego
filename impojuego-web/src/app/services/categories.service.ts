import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiResponse, CategoryDetail, CreateCategoryRequest, UpdateCategoryRequest } from '../models';

@Injectable({
  providedIn: 'root'
})
export class CategoriesService {
  private readonly baseUrl = 'https://impojuego-1.onrender.com/api';
  // private readonly baseUrl = 'http://localhost:5000/api'; // Para desarrollo local

  constructor(private http: HttpClient) {}

  // Obtener categorías del usuario (admin: todas, user: solo propias)
  getCategories(): Observable<CategoryDetail[]> {
    return this.http.get<ApiResponse<CategoryDetail[]>>(`${this.baseUrl}/categories`)
      .pipe(map(res => res.data!));
  }

  // Obtener categorías activas para jugar
  getActiveCategories(): Observable<CategoryDetail[]> {
    return this.http.get<ApiResponse<CategoryDetail[]>>(`${this.baseUrl}/categories/active`)
      .pipe(map(res => res.data!));
  }

  // Obtener una categoría por ID
  getCategory(id: number): Observable<CategoryDetail> {
    return this.http.get<ApiResponse<CategoryDetail>>(`${this.baseUrl}/categories/${id}`)
      .pipe(map(res => res.data!));
  }

  // Crear categoría
  createCategory(request: CreateCategoryRequest): Observable<CategoryDetail> {
    return this.http.post<ApiResponse<CategoryDetail>>(`${this.baseUrl}/categories`, request)
      .pipe(map(res => res.data!));
  }

  // Actualizar categoría
  updateCategory(id: number, request: UpdateCategoryRequest): Observable<CategoryDetail> {
    return this.http.put<ApiResponse<CategoryDetail>>(`${this.baseUrl}/categories/${id}`, request)
      .pipe(map(res => res.data!));
  }

  // Eliminar categoría
  deleteCategory(id: number): Observable<void> {
    return this.http.delete<ApiResponse<any>>(`${this.baseUrl}/categories/${id}`)
      .pipe(map(() => {}));
  }

  // Activar/desactivar categoría
  toggleCategory(id: number): Observable<void> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/categories/${id}/toggle`, {})
      .pipe(map(() => {}));
  }
}
