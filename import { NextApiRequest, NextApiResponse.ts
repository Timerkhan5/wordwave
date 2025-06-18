import { NextApiRequest, NextApiResponse } from 'next';
import prisma from '../../lib/prisma';

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  // Получаем все задания без фильтрации
  const tasks = await prisma.task.findMany();
  res.json(tasks);
}
