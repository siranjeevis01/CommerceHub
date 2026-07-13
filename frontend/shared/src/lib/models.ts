export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  isActive: boolean;
  roles: string[];
  vendorId?: number;
  createdAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
}

export interface Product {
  id: number;
  name: string;
  slug: string;
  description: string;
  shortDescription: string;
  price: number;
  comparePrice?: number;
  sku: string;
  stockQuantity: number;
  imageUrl: string;
  mainImageUrl?: string;
  images: string[];
  galleryImages?: string[];
  categoryId: number;
  categoryName: string;
  brandId: number;
  brandName: string;
  variants: ProductVariant[];
  ratings: number;
  reviewCount: number;
  isActive: boolean;
  tags: string[];
  createdAt: string;
}

export interface ProductVariant {
  id: number;
  productId: number;
  sku: string;
  color?: string;
  size?: string;
  price: number;
  stockQuantity: number;
  imageUrl?: string;
}

export interface Category {
  id: number;
  name: string;
  slug: string;
  description: string;
  imageUrl: string;
  parentId?: number;
  children: Category[];
}

export interface Brand {
  id: number;
  name: string;
  slug: string;
  logoUrl: string;
  isActive: boolean;
}

export interface Cart {
  id: string;
  userId?: number;
  sessionId?: string;
  items: CartItem[];
  total: number;
  itemCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CartItem {
  productId: number;
  variantId?: number;
  name: string;
  imageUrl: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

export interface Order {
  id: number;
  orderNumber: string;
  userId: number;
  status: string;
  totalAmount: number;
  subtotal: number;
  discount: number;
  shippingCost: number;
  tax: number;
  shippingAddress: string;
  paymentMethod: string;
  paymentStatus: string;
  items: OrderItem[];
  tracking: OrderTracking[];
  createdAt: string;
  updatedAt?: string;
}

export interface OrderItem {
  id: number;
  productId: number;
  variantId?: number;
  productName: string;
  imageUrl: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface OrderTracking {
  id: number;
  status: string;
  trackingNumber?: string;
  carrier?: string;
  note: string;
  updatedAt: string;
}

export interface Review {
  id: number;
  productId: number;
  userId: number;
  userName: string;
  rating: number;
  title: string;
  comment: string;
  isApproved: boolean;
  createdAt: string;
}

export interface Address {
  id: number;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  isDefault: boolean;
}

export interface Payment {
  id: number;
  orderId: number;
  amount: number;
  currency: string;
  status: string;
  paymentMethod: string;
  transactionId: string;
}

export interface Coupon {
  id: number;
  code: string;
  discountType: string;
  discountValue: number;
  minimumOrderAmount?: number;
  usedCount: number;
  maxUsageCount?: number;
  expiresAt?: string;
  isValid: boolean;
}

export interface VendorProfile {
  id: number;
  userId: number;
  storeName: string;
  storeSlug: string;
  description: string;
  logoUrl: string;
  bannerUrl: string;
  email: string;
  phone: string;
  address: string;
  status: string;
  rating: number;
  productCount: number;
}

export interface VendorPayout {
  id: number;
  vendorId: number;
  amount: number;
  status: string;
  payoutNumber: string;
  createdAt: string;
  processedAt?: string;
}

export interface Notification {
  type: string;
  title: string;
  message: string;
  orderId?: number;
  orderNumber?: string;
  timestamp: string;
}

export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}
