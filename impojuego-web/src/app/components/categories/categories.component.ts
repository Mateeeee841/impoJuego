import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CategoriesService } from '../../services/categories.service';
import { CategoryDetail } from '../../models';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss'
})
export class CategoriesComponent implements OnInit {
  categories: CategoryDetail[] = [];
  loading = true;
  error = '';

  // Modal state
  showModal = false;
  isEditing = false;
  editingCategoryId: number | null = null;
  categoryName = '';
  categoryWords = '';
  modalLoading = false;
  modalError = '';

  // Delete confirmation
  showDeleteConfirm = false;
  categoryToDelete: CategoryDetail | null = null;

  constructor(
    public authService: AuthService,
    private categoriesService: CategoriesService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading = true;
    this.error = '';

    this.categoriesService.getCategories().subscribe({
      next: (categories) => {
        this.categories = categories;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al cargar categorías';
        this.loading = false;
      }
    });
  }

  openCreateModal(): void {
    this.isEditing = false;
    this.editingCategoryId = null;
    this.categoryName = '';
    this.categoryWords = '';
    this.modalError = '';
    this.showModal = true;
  }

  openEditModal(category: CategoryDetail): void {
    this.isEditing = true;
    this.editingCategoryId = category.id;
    this.categoryName = category.name;
    this.categoryWords = category.words.join('\n');
    this.modalError = '';
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.modalError = '';
  }

  saveCategory(): void {
    const name = this.categoryName.trim();
    const words = this.categoryWords
      .split('\n')
      .map(w => w.trim())
      .filter(w => w.length > 0);

    if (!name) {
      this.modalError = 'El nombre es requerido';
      return;
    }

    if (words.length === 0) {
      this.modalError = 'Agregá al menos una palabra';
      return;
    }

    this.modalLoading = true;
    this.modalError = '';

    if (this.isEditing && this.editingCategoryId) {
      this.categoriesService.updateCategory(this.editingCategoryId, { name, words }).subscribe({
        next: () => {
          this.modalLoading = false;
          this.closeModal();
          this.loadCategories();
        },
        error: (err) => {
          this.modalError = err.error?.message || 'Error al actualizar';
          this.modalLoading = false;
        }
      });
    } else {
      this.categoriesService.createCategory({
        name,
        words
      }).subscribe({
        next: () => {
          this.modalLoading = false;
          this.closeModal();
          this.loadCategories();
        },
        error: (err) => {
          this.modalError = err.error?.message || 'Error al crear';
          this.modalLoading = false;
        }
      });
    }
  }

  confirmDelete(category: CategoryDetail): void {
    this.categoryToDelete = category;
    this.showDeleteConfirm = true;
  }

  cancelDelete(): void {
    this.categoryToDelete = null;
    this.showDeleteConfirm = false;
  }

  deleteCategory(): void {
    if (!this.categoryToDelete) return;

    this.categoriesService.deleteCategory(this.categoryToDelete.id).subscribe({
      next: () => {
        this.cancelDelete();
        this.loadCategories();
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al eliminar';
        this.cancelDelete();
      }
    });
  }

  toggleActive(category: CategoryDetail): void {
    this.categoriesService.toggleCategory(category.id).subscribe({
      next: () => {
        category.isActive = !category.isActive;
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al cambiar estado';
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/lobby']);
  }
}
