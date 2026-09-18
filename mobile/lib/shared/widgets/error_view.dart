import 'package:flutter/material.dart';

/// Consistent inline error display for failure states.
class ErrorView extends StatelessWidget {
  const ErrorView({super.key, required this.message, this.onRetry});

  final String message;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(
          message,
          key: const Key('errorMessage'),
          style: TextStyle(color: Theme.of(context).colorScheme.error),
        ),
        if (onRetry != null) ...[
          const SizedBox(height: 8),
          TextButton(onPressed: onRetry, child: const Text('Retry')),
        ],
      ],
    );
  }
}
