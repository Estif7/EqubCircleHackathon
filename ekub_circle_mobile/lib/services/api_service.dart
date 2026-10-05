import 'dart:convert';
import 'dart:io' show Platform;
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../models/models.dart';

class ApiException implements Exception {
  final String message;
  final int? statusCode;

  ApiException(this.message, [this.statusCode]);

  @override
  String toString() => message;
}

class ApiService {
  static const String _defaultUrlKey = 'ekub_api_base_url';

  static String getDefaultBaseUrl() {
    if (kIsWeb) {
      return 'http://localhost:5000';
    }
    try {
      if (Platform.isAndroid) {
        return 'http://10.0.2.2:5000';
      }
    } catch (_) {}
    return 'http://localhost:5000';
  }

  String _baseUrl = getDefaultBaseUrl();
  String? _token;

  String get baseUrl => _baseUrl;

  void setToken(String? token) {
    _token = token;
  }

  Future<void> init() async {
    final prefs = await SharedPreferences.getInstance();
    final savedUrl = prefs.getString(_defaultUrlKey);
    if (savedUrl != null && savedUrl.isNotEmpty) {
      _baseUrl = savedUrl;
    }
  }

  Future<void> updateBaseUrl(String newUrl) async {
    var cleanUrl = newUrl.trim();
    if (cleanUrl.endsWith('/')) {
      cleanUrl = cleanUrl.substring(0, cleanUrl.length - 1);
    }
    _baseUrl = cleanUrl;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_defaultUrlKey, _baseUrl);
  }

  Map<String, String> _headers() {
    final headers = {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    };
    if (_token != null && _token!.isNotEmpty) {
      headers['Authorization'] = 'Bearer $_token';
    }
    return headers;
  }

  dynamic _processResponse(http.Response response) {
    dynamic body;
    try {
      if (response.body.isNotEmpty) {
        body = jsonDecode(response.body);
      }
    } catch (_) {}

    if (response.statusCode >= 200 && response.statusCode < 300) {
      return body;
    }

    String message = 'Request failed (${response.statusCode})';
    if (body is Map) {
      if (body['message'] != null) {
        message = body['message'].toString();
      } else if (body['title'] != null) {
        message = body['title'].toString();
      } else if (body['errors'] != null && body['errors'] is Map) {
        final errMap = body['errors'] as Map;
        final allErrors = errMap.values.expand((e) => e is List ? e : [e]).join(' ');
        if (allErrors.isNotEmpty) message = allErrors;
      }
    } else if (response.statusCode == 401) {
      message = 'Unauthorized. Please sign in again.';
    }

    throw ApiException(message, response.statusCode);
  }

  // --- AUTH ENDPOINTS ---

  Future<RegisterResponse> register({
    required String fullName,
    required String phoneNumber,
    required String email,
    required String password,
    required String confirmPassword,
    required String faydaFanNumber,
  }) async {
    final url = Uri.parse('$_baseUrl/api/auth/register');
    final response = await http.post(
      url,
      headers: _headers(),
      body: jsonEncode({
        'fullName': fullName,
        'phoneNumber': phoneNumber,
        'email': email,
        'password': password,
        'confirmPassword': confirmPassword,
        'faydaFanNumber': faydaFanNumber,
      }),
    );
    final data = _processResponse(response);
    return RegisterResponse.fromJson(data);
  }

  Future<AuthResponse> verifyOtp(int userId, String otp) async {
    final url = Uri.parse('$_baseUrl/api/auth/verify-otp');
    final response = await http.post(
      url,
      headers: _headers(),
      body: jsonEncode({'userId': userId, 'otp': otp}),
    );
    final data = _processResponse(response);
    return AuthResponse.fromJson(data);
  }

  Future<AuthResponse> login(String emailOrPhone, String password) async {
    final url = Uri.parse('$_baseUrl/api/auth/login');
    final response = await http.post(
      url,
      headers: _headers(),
      body: jsonEncode({'emailOrPhone': emailOrPhone, 'password': password}),
    );
    final data = _processResponse(response);
    return AuthResponse.fromJson(data);
  }

  // --- DASHBOARD ENDPOINTS ---

  Future<DashboardSummary> getDashboardSummary() async {
    final url = Uri.parse('$_baseUrl/api/dashboard/summary');
    final response = await http.get(url, headers: _headers());
    final data = _processResponse(response);
    return DashboardSummary.fromJson(data ?? {});
  }

  // --- CIRCLES ENDPOINTS ---

  Future<List<CircleSummary>> getMyCircles() async {
    final url = Uri.parse('$_baseUrl/api/circles/mine');
    final response = await http.get(url, headers: _headers());
    final data = _processResponse(response) as List<dynamic>? ?? [];
    return data.map((json) => CircleSummary.fromJson(json as Map<String, dynamic>)).toList();
  }

  Future<List<CircleSummary>> getAvailableCircles({String search = '', String frequency = ''}) async {
    final queryParams = <String, String>{};
    if (search.isNotEmpty) queryParams['search'] = search;
    if (frequency.isNotEmpty) queryParams['frequency'] = frequency;

    final url = Uri.parse('$_baseUrl/api/circles/available').replace(queryParameters: queryParams.isEmpty ? null : queryParams);
    final response = await http.get(url, headers: _headers());
    final data = _processResponse(response) as List<dynamic>? ?? [];
    return data.map((json) => CircleSummary.fromJson(json as Map<String, dynamic>)).toList();
  }

  Future<CircleDetails> getCircleDetails(int id) async {
    final url = Uri.parse('$_baseUrl/api/circles/$id');
    final response = await http.get(url, headers: _headers());
    final data = _processResponse(response);
    return CircleDetails.fromJson(data);
  }

  Future<CircleSummary> createCircle({
    required String name,
    required double contributionAmount,
    required String frequency,
    required int memberLimit,
    DateTime? startDate,
  }) async {
    final url = Uri.parse('$_baseUrl/api/circles');
    final body = {
      'name': name,
      'contributionAmount': contributionAmount,
      'frequency': frequency,
      'memberLimit': memberLimit,
      'startDate': startDate?.toUtc().toIso8601String(),
    };
    final response = await http.post(url, headers: _headers(), body: jsonEncode(body));
    final data = _processResponse(response);
    return CircleSummary.fromJson(data);
  }

  Future<void> joinCircle(int id) async {
    final url = Uri.parse('$_baseUrl/api/circles/$id/join');
    final response = await http.post(url, headers: _headers(), body: jsonEncode({}));
    _processResponse(response);
  }

  Future<void> addMember(int id, String emailOrPhone) async {
    final url = Uri.parse('$_baseUrl/api/circles/$id/members');
    final response = await http.post(url, headers: _headers(), body: jsonEncode({'emailOrPhone': emailOrPhone}));
    _processResponse(response);
  }

  Future<void> startCircle(int id) async {
    final url = Uri.parse('$_baseUrl/api/circles/$id/start');
    final response = await http.post(url, headers: _headers(), body: jsonEncode({}));
    _processResponse(response);
  }

  // --- ROUNDS & PAYMENTS ENDPOINTS ---

  Future<Round?> getCurrentRound(int circleId) async {
    final url = Uri.parse('$_baseUrl/api/circles/$circleId/current-round');
    try {
      final response = await http.get(url, headers: _headers());
      if (response.statusCode == 404 || response.statusCode == 204) return null;
      final data = _processResponse(response);
      return data != null ? Round.fromJson(data) : null;
    } catch (_) {
      return null;
    }
  }

  Future<List<Round>> getRounds(int circleId) async {
    final url = Uri.parse('$_baseUrl/api/circles/$circleId/rounds');
    final response = await http.get(url, headers: _headers());
    final data = _processResponse(response) as List<dynamic>? ?? [];
    return data.map((json) => Round.fromJson(json as Map<String, dynamic>)).toList();
  }

  Future<List<HistoryItem>> getHistory(int circleId) async {
    final url = Uri.parse('$_baseUrl/api/circles/$circleId/history');
    final response = await http.get(url, headers: _headers());
    final data = _processResponse(response) as List<dynamic>? ?? [];
    return data.map((json) => HistoryItem.fromJson(json as Map<String, dynamic>)).toList();
  }

  Future<void> recordPayment(int roundId, int membershipId, double amount, {String referenceNote = 'Payment'}) async {
    final url = Uri.parse('$_baseUrl/api/rounds/$roundId/payments/$membershipId');
    final response = await http.post(
      url,
      headers: _headers(),
      body: jsonEncode({
        'amount': amount,
        'referenceNote': referenceNote,
        'paidAt': DateTime.now().toUtc().toIso8601String(),
      }),
    );
    _processResponse(response);
  }

  Future<PayoutResponse> payout(int circleId) async {
    final url = Uri.parse('$_baseUrl/api/circles/$circleId/payout');
    final response = await http.post(url, headers: _headers(), body: jsonEncode({}));
    final data = _processResponse(response);
    return PayoutResponse.fromJson(data);
  }
}

